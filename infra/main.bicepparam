// Parameters file for main.bicep – safe to commit (no secrets here).
// Secrets are passed via GitHub Actions using --parameters overrides.
using 'main.bicep'

param appName = 'ecommercecenter' // REPLACE_ME – lowercase, no spaces, globally unique prefix
param skuName = 'F1'              // Free tier – change to B1 for alwaysOn support
param jwtIssuer = 'ECommerceCenter'
param jwtAudience = 'ECommerceCenterClient'
param smtpHost = 'smtp.gmail.com'
param smtpPort = '587'
param smtpUsername = ''            // REPLACE_ME – your SMTP email
param frontendBaseUrl = 'http://localhost:3000' // REPLACE_ME – your deployed frontend URL
