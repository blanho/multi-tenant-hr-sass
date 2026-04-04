#!/usr/bin/env bash
set -euo pipefail

APP_URL="${1:?Usage: smoke-test.sh <base-url> [max-attempts] [sleep-seconds]}"
MAX_ATTEMPTS="${2:-10}"
SLEEP_SECONDS="${3:-15}"
STARTUP_DELAY="${4:-45}"

echo "=== HrSaas Smoke Test ==="
echo "Target:       $APP_URL"
echo "Max attempts: $MAX_ATTEMPTS"
echo "Sleep:        ${SLEEP_SECONDS}s"
echo "Startup wait: ${STARTUP_DELAY}s"
echo ""

echo "Waiting ${STARTUP_DELAY}s for application startup..."
sleep "$STARTUP_DELAY"

for i in $(seq 1 "$MAX_ATTEMPTS"); do
    LIVE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$APP_URL/health/live" 2>/dev/null || echo "000")
    READY=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$APP_URL/health/ready" 2>/dev/null || echo "000")

    echo "Attempt $i/$MAX_ATTEMPTS: live=$LIVE ready=$READY"

    if [ "$LIVE" = "200" ] && [ "$READY" = "200" ]; then
        echo ""
        echo "=== Smoke Test PASSED ==="

        echo "Verifying API endpoints..."
        AUTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$APP_URL/api/v1/auth/me" 2>/dev/null || echo "000")
        echo "  GET /api/v1/auth/me -> $AUTH_STATUS (expected: 401)"

        echo ""
        echo "=== All checks completed ==="
        exit 0
    fi

    if [ "$i" -lt "$MAX_ATTEMPTS" ]; then
        sleep "$SLEEP_SECONDS"
    fi
done

echo ""
echo "=== Smoke Test FAILED ==="
echo "Application did not become healthy after $MAX_ATTEMPTS attempts"
exit 1
