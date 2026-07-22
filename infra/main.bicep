// Infrastructure MealPlanner — un environnement (dev ou prod) par déploiement.
// Stack « tout gratuit » : Azure SQL serverless (offre gratuite, auto-pause) + Container App
// scale-to-zero. Image conteneur tirée depuis ghcr.io (publique). Auth SQL en Managed Identity
// (aucun mot de passe) : le user-assigné du Container App reçoit ses droits via une étape T-SQL
// du pipeline (CREATE USER FROM EXTERNAL PROVIDER).

@description('Nom court de l\'environnement (dev, prod).')
@allowed(['dev', 'prod'])
param environmentName string

@description('Région Azure.')
param location string = 'francecentral'

@description('Image conteneur complète à déployer (ex. ghcr.io/mika-l/mealplanner:<sha>).')
param containerImage string

@description('Clé de signature JWT (>= 32 octets). Injectée comme secret du Container App.')
@secure()
param jwtSigningKey string

@description('Login de l\'administrateur Entra du serveur SQL (nom d\'affichage du principal déployeur).')
param sqlAdminLogin string

@description('Object ID (SID) du principal Entra administrateur du serveur SQL.')
param sqlAdminObjectId string

@description('Type de principal administrateur Entra.')
@allowed(['User', 'Group', 'Application'])
param sqlAdminPrincipalType string = 'Application'

var namePrefix = 'mealplanner-${environmentName}'
var databaseName = 'mealplanner'
var uniqueSuffix = uniqueString(resourceGroup().id)
var sqlServerName = 'sql-${namePrefix}-${uniqueSuffix}'

// --- Identité managée (Container App -> SQL) ---
resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${namePrefix}'
  location: location
}

// --- Journaux (requis par l'environnement Container Apps) ---
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${namePrefix}'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

// --- Environnement Container Apps ---
resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${namePrefix}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// --- Serveur SQL (Entra-only, admin = principal déployeur) ---
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: sqlAdminPrincipalType
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
  }
}

// La règle 0.0.0.0 ouvre le port 1433 à TOUT service Azure (tous tenants, toutes souscriptions),
// pas seulement à notre Container App. Trade-off accepté : le serveur est en `azureADOnlyAuthentication`,
// donc aucun mot de passe SQL à brute-forcer — se connecter exige un token Entra de NOTRE tenant émis
// pour un principal ayant des droits sur la base. Pour durcir (isoler au seul Container App), il faudrait
// un private endpoint ou une IP sortante statique via VNet — tous deux payants, ce qui casse la stack « tout gratuit ».
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// --- Base SQL serverless, offre gratuite (auto-pause à l'épuisement du quota mensuel) ---
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 2
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    maxSizeBytes: 34359738368 // 32 Go
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    zoneRedundant: false
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

var sqlConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${databaseName};Authentication=Active Directory Managed Identity;User Id=${identity.properties.clientId};Encrypt=True;'

// --- Container App (scale-to-zero, image ghcr publique) ---
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-${namePrefix}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      secrets: [
        {
          name: 'jwt-signing-key'
          value: jwtSigningKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'ASPNETCORE_HTTP_PORTS', value: '8080' }
            { name: 'ConnectionStrings__MealsDb', value: sqlConnectionString }
            { name: 'ConnectionStrings__IdentityDb', value: sqlConnectionString }
            { name: 'Jwt__SigningKey', secretRef: 'jwt-signing-key' }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = databaseName
output managedIdentityName string = identity.name
output managedIdentityClientId string = identity.properties.clientId
