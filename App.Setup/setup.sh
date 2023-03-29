#!/usr/bin/env bash

set -e

cd "$(dirname "$(realpath "$0")")";

# Load env variables
set -a
source .env
test -f .env.local && source .env.local
set +a

# Create google project if not exists
if ! gcloud projects describe "${APP_SLUG}" > /dev/null; then
  echo "Creating project: ${APP_SLUG}"
  gcloud projects create "${APP_SLUG}" --name "${PROJECT_NAME}"
else
  echo "Project already exists: ${APP_SLUG}"
fi
echo ""

# Link billing account
GOOGLE_BILLING_ACCOUNT_FIRST=$(gcloud beta billing accounts list --filter="open=true" --format="value(name)" --limit=1)
GOOGLE_BILLING_ACCOUNT_ID=${GOOGLE_BILLING_ACCOUNT_ID:-${GOOGLE_BILLING_ACCOUNT_FIRST}}
echo "Ensure project ${APP_SLUG} is linked to billing account ${GOOGLE_BILLING_ACCOUNT_ID}"
gcloud beta billing projects link "${APP_SLUG}" "--billing-account=${GOOGLE_BILLING_ACCOUNT_ID}"
echo ""


# Generate pulumi config pass and local state file
if [ -z "${PULUMI_CONFIG_PASSPHRASE}" ]; then
    PULUMI_CONFIG_PASSPHRASE=$(echo $RANDOM | md5sum | head -c 20; echo;)
    echo "PULUMI_CONFIG_PASSPHRASE='${PULUMI_CONFIG_PASSPHRASE}'" >> .env.local    
    echo "Generated PULUMI_CONFIG_PASSPHRASE"
fi


# Login to pulumi
pulumi login --local

# Set pulumi configuration only if modified
if [ ".env" -nt "Pulumi.setup.yaml" ] \
  || [ ".env.local" -nt "Pulumi.setup.yaml" ] \
  || [ ! -f "Pulumi.setup.yaml" ]
then
    pulumi stack select \
        --non-interactive \
        --create "setup"
    
    # Set configurations
    function pulumi_config_set {
        echo "Setting config ${1}"
        pulumi config set --non-interactive "${@}"
    }
    
    pulumi_config_set app:slug "${APP_SLUG}"
    pulumi_config_set app:repository "${APP_REPOSITORY}"
    pulumi_config_set app:repositoryIsOrganization "${APP_REPOSITORY_IS_ORGANISATION}"
    pulumi_config_set app:region "${GCP_REGION}"
    
    pulumi_config_set gcp:project "${APP_SLUG}"
    pulumi_config_set gcp:region "${GCP_REGION}"
    
    pulumi_config_set github:token "${GITHUB_TOKEN}" --secret
else
    pulumi stack select \
        --non-interactive \
        --create "setup"
fi

pulumi refresh \
    --non-interactive \
    --yes \
    --clear-pending-creates \
    --diff

pulumi up \
    --non-interactive \
    --yes \
    --show-full-output \
    --show-config \
    --diff \
    "${@:2}"