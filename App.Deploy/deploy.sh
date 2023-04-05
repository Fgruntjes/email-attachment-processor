#!/usr/bin/env bash

set -e
 
cd "$(dirname "$(realpath "$0")")";

set -a
test -f .env.deploy.local && source .env.deploy.local
test -f .env.local && source .env.local
set +a

# Login to cloud
pulumi login "gs://${GOOGLE_PROJECT_ID}-pulumi"

# Set configuration only if modified
if { [ ".env.deploy.local" -nt "Pulumi.${APP_ENVIRONMENT}.yaml" ] || [ ".env.local" -nt "Pulumi.${APP_ENVIRONMENT}.yaml" ]; } || [ ! -f "Pulumi.${APP_ENVIRONMENT}.yaml" ]; then
    # So old configs are cleared
    rm -f "Pulumi.${APP_ENVIRONMENT}.yaml"
    
    pulumi stack select \
        --non-interactive \
        --create "${APP_ENVIRONMENT}"
    
    # Set configurations
    function pulumi_config_set {
        echo "Setting config ${1}"
        pulumi config set --non-interactive "${@}"
    }
    
    pulumi_config_set gcp:project "${GOOGLE_PROJECT_ID}"
    pulumi_config_set gcp:region "${GOOGLE_REGION}"
    
    pulumi_config_set app:tag "${APP_TAG}"
    pulumi_config_set app:environment "${APP_ENVIRONMENT}"
    pulumi_config_set app:projectDir "$(dirname $(pwd))"

    pulumi_config_set sentry:dsn "${SENTRY_DSN}" --secret
else
    pulumi stack select \
        --non-interactive \
        --create "${APP_ENVIRONMENT}"
fi

# Concurrency is handled by github actions
pulumi cancel --yes


if [ "$1" == "up" ]; then
    pulumi up \
      --non-interactive \
      --yes \
      --show-full-output \
      --show-config \
      --diff \
      "${@:2}"
elif [ "$1" == "rm" ]; then
    pulumi destroy --yes
    pulumi stack rm --yes
else 
    echo "deploy.sh [up|rm]"
    exit 1
fi