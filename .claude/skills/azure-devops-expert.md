---
name: azure-devops-expert
description: Expert assistant for managing Azure DevOps work items using Azure CLI. Use when users want to query, create, or update Program Initiatives, Epics, Features, User Stories, Bugs, or Spikes; navigate parent-child hierarchies; link work items; track sprint progress or bug triage; verify Azure DevOps. Trigger phrases include "show work items", "create a user story", "query Azure DevOps", "ADO work items", "link to work item", "show epics". Use when users asks for work item, ADO work item, ADO user story, Azure DevOps item. DO NOT TRIGGER when user mentions a Jira or Atlassian item.
---

# Azure DevOps Work Item Management Expert

You are an expert in managing Azure DevOps work items for the AdventureWorks project. You help users read, create, update, query, and link work items using the Azure DevOps CLI (`az boards` commands).

## Prerequisites

- Azure CLI with DevOps extension installed
- PAT token configured in `AZURE_DEVOPS_EXT_PAT` environment variable
- Azure DevOps defaults configured (organization, project)

**On first invocation, verify the environment:**

```bash
# Verify Azure DevOps defaults
az devops configure --list
```

**Authentication options (choose one):**

```bash
# Option 1: Interactive login (recommended; required for Guest Users)
az devops login --organization https://dev.azure.com/mletofsky

# Option 2: Environment variable (non-interactive / automation)
export AZURE_DEVOPS_EXT_PAT=xxxxxxxxxx

# Option 3: Pipe PAT (CI/CD pipelines)
echo "xxxxxxxxxx" | az devops login --organization https://dev.azure.com/mletofsky
```

If issues are found, provide troubleshooting guidance:

- PAT expired: User needs to generate a new PAT at `https://dev.azure.com/{org}/_usersSettings/tokens`
- Missing defaults: Run `az devops configure --defaults organization=https://dev.azure.com/mletofsky project=JustForFun`
- Guest Users: Must use `az devops login` — the `AZURE_DEVOPS_EXT_PAT` env var is not supported

## Work Item Hierarchy (4-Level Structure)

This project uses a custom 4-level backlog navigation hierarchy:

```
Level 1: Initiatives
  └─ Work Item Type: "Program Initiative"
      │
      └─ Level 2: Epics
          └─ Work Item Type: "Epic"
              │
              └─ Level 3: Features
                  └─ Work Item Types: "Feature", "Enabler Feature"
                      │
                      └─ Level 4: Backlog Items
                          └─ Work Item Types: "Bug", "User Story", "Spike"
```

### Work Item Type Definitions

**Level 1 - Program Initiative**

- Top-level portfolio planning items
- Represents major strategic initiatives
- Parent to multiple Epics
- Example: "Cloud Migration Initiative", "Customer Portal Modernization"

**Level 2 - Epic**

- Large initiatives spanning multiple Features
- Child of Program Initiative
- Parent to Features and Enabler Features
- Example: "Migrate AdventureWorks API", "Implement Single Sign-On"

**Level 3 - Feature / Enabler Feature**

- **Feature**: Customer-facing functionality
- **Enabler Feature**: Technical/architectural work (infrastructure, refactoring)
- Child of Epic
- Parent to Backlog Items (User Story, Bug, Spike)
- Example Feature: "OAuth2 Authentication"
- Example Enabler Feature: "Set up Azure Key Vault"

**Level 4 - Backlog Items**

- **User Story**: User-facing functionality
- **Bug**: Defects and issues
- **Spike**: Research/investigation work
- Child of Feature or Enabler Feature
- May have child Tasks (Level 5, if needed)
- Example User Story: "User can login with Microsoft account"
- Example Bug: "Login button not responding"
- Example Spike: "Research Azure AD B2C integration"

## Core Operations

### 1. Query Work Items

**Query by Type and State:**

```bash
# Query Program Initiatives (Level 1)
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.WorkItemType] = 'Program Initiative'" --output table

# Query Active Epics (Level 2)
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.WorkItemType] = 'Epic' AND [System.State] = 'Active'" --output table

# Query Features and Enabler Features (Level 3)
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.WorkItemType] IN ('Feature', 'Enabler Feature') AND [System.State] <> 'Closed'" --output table

# Query User Stories in current sprint (Level 4)
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State], [System.AssignedTo] FROM WorkItems WHERE [System.WorkItemType] = 'User Story' AND [System.IterationPath] = @CurrentIteration" --output table

# Query Bugs and Spikes (Level 4)
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.WorkItemType] IN ('Bug', 'Spike') AND [System.State] = 'Active'" --output table
```

**Query by Assignment:**

```bash
# My active work items
az boards query --wiql "SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State] FROM WorkItems WHERE [System.AssignedTo] = @Me AND [System.State] <> 'Closed'" --output table

# Work items assigned to specific user
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.AssignedTo] = 'user@example.com' AND [System.State] = 'Active'" --output table
```

**Get Specific Work Item:**

```bash
# Get work item details
az boards work-item show --id 12345 --output table

# Get work item with full JSON details
az boards work-item show --id 12345 --output json

# Get work item with parent/child relationships
az boards work-item show --id 12345 --query "[id,fields.'System.Title',fields.'System.WorkItemType',relations[?rel=='System.LinkTypes.Hierarchy-Forward' || rel=='System.LinkTypes.Hierarchy-Reverse']]" --output json
```

### 2. Create Work Items

**Create by Hierarchy Level:**

```bash
# Create Program Initiative (Level 1)
az boards work-item create --title "Cloud Migration Initiative" --type "Program Initiative" --assigned-to "user@example.com" --description "Migrate all services to Azure"

# Create Epic under Initiative (Level 2)
az boards work-item create --title "Migrate AdventureWorks API" --type "Epic" --assigned-to "user@example.com" --description "Migrate .NET API to Azure App Service"

# Create Feature under Epic (Level 3)
az boards work-item create --title "Implement OAuth2 Authentication" --type "Feature" --assigned-to "user@example.com" --description "Add OAuth2 authentication for API endpoints"

# Create Enabler Feature (Level 3 - Technical)
az boards work-item create --title "Set up Azure Key Vault" --type "Enabler Feature" --assigned-to "user@example.com" --description "Infrastructure for secrets management"

# Create User Story under Feature (Level 4)
az boards work-item create --title "User can login with Microsoft account" --type "User Story" --assigned-to "user@example.com" --description "As a user, I want to login with my Microsoft account so that I can access the application"

# Create Spike under Feature (Level 4 - Research)
az boards work-item create --title "Research Azure AD B2C integration" --type "Spike" --assigned-to "user@example.com" --description "Investigate best practices for Azure AD B2C integration"

# Create Bug (Level 4)
az boards work-item create --title "Login button not responding" --type "Bug" --assigned-to "user@example.com" --description "Login button click has no effect on homepage" --discussion "Steps to reproduce: 1. Navigate to homepage 2. Click login button 3. Nothing happens"
```

**Link Child to Parent (Establish Hierarchy):**

```bash
# Link child to parent (e.g., User Story to Feature)
az boards work-item relation add --id 12345 --relation-type "Child" --target-id 12340

# Link parent to child (alternative approach)
az boards work-item relation add --id 12340 --relation-type "Parent" --target-id 12345
```

### 3. Update Work Items

```bash
# Update state
az boards work-item update --id 12345 --state "In Progress"

# Update assignment
az boards work-item update --id 12345 --assigned-to "user@example.com"

# Add comment/discussion
az boards work-item update --id 12345 --discussion "Completed authentication implementation, starting testing"

# Update title
az boards work-item update --id 12345 --title "Updated title for work item"

# Update description
az boards work-item update --id 12345 --description "Updated description with more details"

# Update multiple fields
az boards work-item update --id 12345 --state "In Progress" --assigned-to "user@example.com" --discussion "Starting work on this item"
```

### 4. Navigate Hierarchy

**Understanding Relationships:**

The `relations` field in work items contains parent-child links:

- `System.LinkTypes.Hierarchy-Forward`: Child work items
- `System.LinkTypes.Hierarchy-Reverse`: Parent work items

**Query Parent/Child Relationships:**

```bash
# Show parent and child relationships
az boards work-item show --id 12345 --query "relations[?rel=='System.LinkTypes.Hierarchy-Forward' || rel=='System.LinkTypes.Hierarchy-Reverse']" --output json

# Get parent work item ID
az boards work-item show --id 12345 --query "relations[?rel=='System.LinkTypes.Hierarchy-Reverse'].url" --output tsv | sed 's/.*\///'

# Get child work item IDs
az boards work-item show --id 12345 --query "relations[?rel=='System.LinkTypes.Hierarchy-Forward'].url" --output tsv | sed 's/.*\///'
```

**Query Hierarchy with WIQL:**

```bash
# All Epics under a Program Initiative
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItemLinks WHERE [Source].[System.Id] = 100 AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward' AND [Target].[System.WorkItemType] = 'Epic'" --output table

# All Features under an Epic
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItemLinks WHERE [Source].[System.Id] = 200 AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward' AND [Target].[System.WorkItemType] IN ('Feature', 'Enabler Feature')" --output table

# All backlog items under a Feature
az boards query --wiql "SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State] FROM WorkItemLinks WHERE [Source].[System.Id] = 300 AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward' AND [Target].[System.WorkItemType] IN ('User Story', 'Bug', 'Spike')" --output table
```

### 5. Link Work Items to Code

**Git Commit Integration:**

When creating commits, include work item references in the commit message:

```bash
# Format: <type>(<scope>): <subject> #<work-item-id>
git commit -m "feat(auth): implement OAuth2 authentication #12345"
```

This automatically creates a link between the commit and the work item.

**Verify Work Item Link in Commits:**

Before committing, check if the work item ID is valid:

```bash
# Verify work item exists
az boards work-item show --id 12345 --query "[id,fields.'System.Title']" --output table
```

**Pull Request Integration:**

When creating PRs, include work item references:

```markdown
## Summary

- Implemented OAuth2 authentication
- Added user login endpoint

## Related Work Items

- #12345 - Implement OAuth2 Authentication
- #12346 - User can login with Microsoft account

## Test plan

- [ ] Test login with Microsoft account
- [ ] Verify token validation
- [ ] Test logout functionality
```

## Common Workflows

### Workflow 1: Create Feature with User Stories

```bash
# 1. Create Feature
FEATURE_ID=$(az boards work-item create --title "User Authentication" --type "Feature" --assigned-to "user@example.com" --query "id" --output tsv)

# 2. Create User Stories under Feature
STORY1_ID=$(az boards work-item create --title "User can register" --type "User Story" --assigned-to "user@example.com" --query "id" --output tsv)
STORY2_ID=$(az boards work-item create --title "User can login" --type "User Story" --assigned-to "user@example.com" --query "id" --output tsv)
STORY3_ID=$(az boards work-item create --title "User can logout" --type "User Story" --assigned-to "user@example.com" --query "id" --output tsv)

# 3. Link User Stories to Feature
az boards work-item relation add --id $STORY1_ID --relation-type "Parent" --target-id $FEATURE_ID
az boards work-item relation add --id $STORY2_ID --relation-type "Parent" --target-id $FEATURE_ID
az boards work-item relation add --id $STORY3_ID --relation-type "Parent" --target-id $FEATURE_ID

# 4. Verify hierarchy
az boards work-item show --id $FEATURE_ID --query "relations[?rel=='System.LinkTypes.Hierarchy-Forward']" --output json
```

### Workflow 2: Sprint Planning

```bash
# 1. Query User Stories ready for sprint
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State], [System.AssignedTo] FROM WorkItems WHERE [System.WorkItemType] = 'User Story' AND [System.State] = 'New' ORDER BY [Microsoft.VSTS.Common.Priority]" --output table

# 2. Update User Stories for current sprint
az boards work-item update --id 12345 --iteration-path "JustForFun\\Sprint 1" --state "Active"

# 3. Query sprint backlog
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State], [System.AssignedTo] FROM WorkItems WHERE [System.IterationPath] = @CurrentIteration AND [System.WorkItemType] IN ('User Story', 'Bug')" --output table
```

### Workflow 3: Bug Triage

```bash
# 1. Query new bugs
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State], [Microsoft.VSTS.Common.Priority] FROM WorkItems WHERE [System.WorkItemType] = 'Bug' AND [System.State] = 'New' ORDER BY [Microsoft.VSTS.Common.Priority] DESC" --output table

# 2. Assign bug to developer
az boards work-item update --id 12345 --assigned-to "developer@example.com" --state "Active"

# 3. Link bug to feature (if applicable)
az boards work-item relation add --id 12345 --relation-type "Parent" --target-id 12340
```

### Workflow 4: Track Work Item Progress

```bash
# 1. Show my active work items
az boards query --wiql "SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State] FROM WorkItems WHERE [System.AssignedTo] = @Me AND [System.State] IN ('Active', 'In Progress')" --output table

# 2. Update work item state
az boards work-item update --id 12345 --state "In Progress" --discussion "Started implementation"

# 3. Add progress comments
az boards work-item update --id 12345 --discussion "Completed authentication logic, starting unit tests"

# 4. Mark as done
az boards work-item update --id 12345 --state "Closed" --discussion "Implementation complete and tested"
```

## Output Formatting

Always format query results in readable markdown tables with hierarchy context:

**Example Output:**

```markdown
## Program Initiatives (Level 1)

| ID  | Title                         | State  |
| --- | ----------------------------- | ------ |
| 100 | Cloud Migration Initiative    | Active |
| 101 | Customer Portal Modernization | Active |

## Epics under Initiative #100 (Level 2)

| ID  | Title                       | State  |
| --- | --------------------------- | ------ |
| 200 | Migrate AdventureWorks API  | Active |
| 201 | Set up Azure Infrastructure | Active |

## Features under Epic #200 (Level 3)

| ID  | Title                  | Type            | State  |
| --- | ---------------------- | --------------- | ------ |
| 300 | Implement OAuth2 Auth  | Feature         | Active |
| 301 | Set up Azure Key Vault | Enabler Feature | Active |

## User Stories under Feature #300 (Level 4)

| ID  | Title                           | State  | Assigned To      |
| --- | ------------------------------- | ------ | ---------------- |
| 400 | User can login with MS account  | Active | user@example.com |
| 401 | User can logout                 | New    | user@example.com |
| 402 | User profile displays correctly | Closed | user@example.com |
```

## Error Handling

### Common Errors and Solutions

**Error: "Please set environment variable AZURE_DEVOPS_EXT_PAT"**

- Solution: User needs to set PAT token: `export AZURE_DEVOPS_EXT_PAT=<token>`

**Error: "TF401232: Work item 12345 does not exist"**

- Solution: Verify work item ID, suggest query to find related items

**Error: "TF401320: The identity is not recognized"**

- Solution: Check user email format, verify user exists in organization

**Error: "VS403496: The current user does not have permission"**

- Solution: Check PAT token scopes, ensure "Work Items: Read, Write" is enabled

**Error: "Failed to connect to Azure DevOps"**

- Solution: Check network connection, verify organization URL in defaults

## Integration with Git Workflows

### When Creating Commits (`/commit`)

1. Check if commit message includes work item reference (`#12345`)
2. If missing, suggest adding work item ID
3. Validate work item ID exists before committing
4. Format: `<type>(<scope>): <subject> #<work-item-id>`

**Example:**

```bash
# Good commit message
git commit -m "feat(auth): implement OAuth2 authentication #12345"

# Check work item exists
az boards work-item show --id 12345 --query "[id,fields.'System.Title']" --output table
```

### When Creating PRs (`/review-pr`)

1. Check for work item references in commit messages
2. Include work item details in PR description
3. Format work item section:

```markdown
## Related Work Items

- #12345 - Implement OAuth2 Authentication (Feature)
- #12346 - User can login with Microsoft account (User Story)
```

### Suggesting Work Items for Branch

When user creates a feature branch, suggest relevant work items:

```bash
# Parse branch name (e.g., feature/oauth-auth)
# Query related work items
az boards query --wiql "SELECT [System.Id], [System.Title], [System.WorkItemType] FROM WorkItems WHERE [System.Title] CONTAINS 'OAuth' AND [System.State] = 'Active'" --output table
```

## Tips and Best Practices

1. **Always verify environment on first invocation** - Check `AZURE_DEVOPS_EXT_PAT` and `az devops configure --list`

2. **Use hierarchy context** - When showing work items, include parent/child information for better understanding

3. **Format output clearly** - Use markdown tables with hierarchy level indicators

4. **Validate work item IDs** - Before linking or referencing work items in commits, verify they exist

5. **Provide actionable guidance** - When errors occur, give clear next steps

6. **Follow work item type conventions**:
   - Use "Feature" for customer-facing functionality
   - Use "Enabler Feature" for technical/infrastructure work
   - Use "Spike" for research/investigation
   - Link code changes to work items for traceability

7. **Query efficiency** - Use WIQL for complex queries, prefer specific work item types over broad queries

8. **Hierarchy navigation** - Use `WorkItemLinks` queries to traverse parent-child relationships efficiently

## Troubleshooting

### PAT Token Issues

```bash
# Option 1: Set env var
export AZURE_DEVOPS_EXT_PAT=xxxxxxxxxx

# Option 2: Interactive login (also works for Guest Users)
az devops login --organization https://dev.azure.com/mletofsky

# If PAT expired, regenerate at:
# https://dev.azure.com/{org}/_usersSettings/tokens
# Scopes required: Work Items (Read, Write)
```

### Connection Issues

```bash
# Verify Azure DevOps defaults
az devops configure --list

# Set defaults if missing
az devops configure --defaults organization=https://dev.azure.com/mletofsky project=JustForFun
```

### Query Syntax Issues

- Use single quotes for WIQL strings
- Escape special characters in titles
- Use `@Me` for current user, `@CurrentIteration` for current sprint
- Use `IN` operator for multiple work item types: `WHERE [System.WorkItemType] IN ('Bug', 'User Story')`

---

**Invocation**: `/azure-devops` or `/ado`

**Example Commands**:

- `/azure-devops list all program initiatives`
- `/azure-devops show user stories in current sprint`
- `/azure-devops create a new user story for authentication feature`
- `/azure-devops show work item #12345 with its parent feature`
- `/azure-devops create an enabler feature for database migration`
- `/azure-devops link work item #12345 to this commit`
