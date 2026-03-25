# Security Policy

## Scope

This document covers the LocalChat AI orchestration backend (`src/*`, API, infrastructure, and contracts).

## Supported Release Practice

This project is under active development. Treat the latest mainline code as the supported baseline unless your team defines tagged release branches.

## Reporting a Vulnerability

If you discover a security issue:

1. Do not open a public issue with exploit details.
2. Report privately to the maintainers through your designated security channel.
3. Include reproduction steps, impact, and suggested mitigations when possible.

## Current Security Posture Notes

- API authentication/authorization middleware is not enabled by default.
- Admin and maintenance endpoints are available on the same API surface.
- Deploy only in trusted networks or behind a secure gateway until auth controls are added.

## Secrets and Credentials

- Never commit real provider keys/tokens to git history.
- Configure secrets via environment variables or a secrets manager.
- Rotate keys immediately if they are exposed in source, logs, or artifacts.

## Hardening Recommendations

1. Terminate TLS at a trusted edge proxy.
2. Add request throttling/rate limits at gateway level.
3. Restrict access to `/api/admin*` endpoints.
4. Enforce backup encryption and least-privilege filesystem access.
5. Monitor `/health` and error rates for anomaly detection.

