# Security Policy

## Reporting a Vulnerability

**Do not open a public issue.** Use GitHub's [private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability) feature instead.

Include a description, reproduction steps, and potential impact.

## Security Practices

- No secrets in code — Azure Key Vault (prod), User Secrets (dev)
- Microsoft Entra ID authentication on all protected endpoints
- FluentValidation on all API inputs
- Entity Framework Core parameterized queries (no raw SQL concatenation)
- HTTPS enforced in all environments
