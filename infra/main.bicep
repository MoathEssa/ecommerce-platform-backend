// ──────────────────────────────────────────────────────────────────────────────
// ECommerceCenter – Azure App Service Infrastructure (Bicep)
// Deploy: az deployment group create -g <rg> -f infra/main.bicep -p infra/main.parameters.json
// ──────────────────────────────────────────────────────────────────────────────

@description('Base name used to derive all resource names (e.g. ecommercecenter)')
param appName string

@description('Azure region for all resources')
param location string = 'eastus'

@description('App Service Plan SKU – F1 (free) is great for portfolio demos')
@allowed(['F1', 'B1', 'B2', 'S1'])
param skuName string = 'F1'

// ── Secrets / App Settings (injected from GitHub Secrets) ───────────────────

@secure()
@description('SQL / PostgreSQL connection string')
param connectionStringDefault string = ''

@secure()
@description('JWT signing key – at least 32 characters')
param jwtSecretKey string = ''

@description('JWT issuer')
param jwtIssuer string = 'ECommerceCenter'

@description('JWT audience')
param jwtAudience string = 'ECommerceCenterClient'

@secure()
@description('SMTP password / app-password')
param smtpPassword string = ''

@description('SMTP host')
param smtpHost string = 'smtp.gmail.com'

@description('SMTP port')
param smtpPort string = '587'

@description('SMTP username / from-email')
param smtpUsername string = ''

@description('Frontend base URL for CORS')
param frontendBaseUrl string = 'http://localhost:3000'

@secure()
@description('CJ Dropshipping API key')
param cjApiKey string = ''

@secure()
@description('Stripe secret key')
param stripeSecretKey string = ''

@secure()
@description('Stripe webhook signing secret')
param stripeWebhookSecret string = ''

@description('Azure Blob Storage account name')
param blobStorageAccountName string = ''

@secure()
@description('Azure Blob Storage account key')
param blobStorageAccountKey string = ''

// ── App Service Plan ────────────────────────────────────────────────────────

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${appName}-plan'
  location: location
  kind: 'linux'
  sku: {
    name: skuName
  }
  properties: {
    reserved: true // required for Linux
  }
}

// ── App Service ─────────────────────────────────────────────────────────────

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: '${appName}-api'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: skuName != 'F1' // F1 does not support alwaysOn
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        // ── Connection Strings ──
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: connectionStringDefault
        }
        // ── JWT ──
        {
          name: 'JwtSettings__SecretKey'
          value: jwtSecretKey
        }
        {
          name: 'JwtSettings__Issuer'
          value: jwtIssuer
        }
        {
          name: 'JwtSettings__Audience'
          value: jwtAudience
        }
        {
          name: 'JwtSettings__AccessTokenExpirationMinutes'
          value: '30'
        }
        {
          name: 'JwtSettings__RefreshTokenExpirationDays'
          value: '7'
        }
        // ── SMTP ──
        {
          name: 'SmtpSettings__Host'
          value: smtpHost
        }
        {
          name: 'SmtpSettings__Port'
          value: smtpPort
        }
        {
          name: 'SmtpSettings__Username'
          value: smtpUsername
        }
        {
          name: 'SmtpSettings__Password'
          value: smtpPassword
        }
        {
          name: 'SmtpSettings__FromEmail'
          value: smtpUsername
        }
        {
          name: 'SmtpSettings__FromName'
          value: 'ECommerce Center'
        }
        // ── App / CORS ──
        {
          name: 'AppSettings__FrontendBaseUrl'
          value: frontendBaseUrl
        }
        // ── Store ──
        {
          name: 'Store__CurrencyCode'
          value: 'SAR'
        }
        // ── CJ Dropshipping ──
        {
          name: 'CjDropshipping__ApiKey'
          value: cjApiKey
        }
        // ── Stripe ──
        {
          name: 'Stripe__SecretKey'
          value: stripeSecretKey
        }
        {
          name: 'Stripe__WebhookSecret'
          value: stripeWebhookSecret
        }
        // ── Blob Storage ──
        {
          name: 'BlobStorage__AccountName'
          value: blobStorageAccountName
        }
        {
          name: 'BlobStorage__AccountKey'
          value: blobStorageAccountKey
        }
        // ── ASP.NET Core environment ──
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
  }
}

// ── Outputs ─────────────────────────────────────────────────────────────────

output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
