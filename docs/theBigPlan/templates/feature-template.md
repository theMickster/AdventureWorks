# Feature [Epic#].[Feature#]: [Feature Title]

**Epic**: [Link to Epic](../epics/[epic-file].md)
**Status**: Not Started
**Assigned To**: [Developer Name or Team]
**Estimated Story Points**: [X]

---

## User Story

**As a** [role/persona]
**I want** [capability/feature]
**So that** [business value/benefit]

---

## Acceptance Criteria (Gherkin Format)

### Scenario 1: [Scenario Name]

**Given** [initial context/precondition]
**When** [action or event occurs]
**Then** [expected outcome/result]
**And** [additional expected outcome]
**But** [constraint or exception if applicable]

### Scenario 2: [Another Scenario Name]

**Given** [initial context/precondition]
**When** [action or event occurs]
**Then** [expected outcome/result]
**And** [additional expected outcome]

### Scenario 3: [Edge Case or Error Scenario]

**Given** [error condition context]
**When** [action that triggers error]
**Then** [expected error handling behavior]
**And** [user-friendly error message displayed]

---

## Technical Implementation

### Key Files to Create/Modify

- `[path/to/file1.ts]` - [Purpose of this file]
- `[path/to/file2.cs]` - [Purpose of this file]
- `[path/to/file3.tsx]` - [Purpose of this file]

### API Endpoints (if applicable)

- **GET** `/api/[resource]` - [Description]
- **POST** `/api/[resource]` - [Description]
- **PUT** `/api/[resource]/{id}` - [Description]
- **DELETE** `/api/[resource]/{id}` - [Description]

### Dependencies

- **Libraries**: [List of npm/NuGet packages needed]
- **External Services**: [Azure services, third-party APIs]
- **Other Features**: [Features that must be complete before this one]

### Data Models

**TypeScript Interface** (Frontend):
```typescript
export interface [ModelName] {
  id: number;
  name: string;
  // ... other properties
}
```

**C# DTO** (Backend):
```csharp
public class [ModelName]Dto
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ... other properties
}
```

---

## Testing Requirements

### Unit Tests

- **Test 1**: [Description of what to test]
  - Given: [Setup]
  - When: [Action]
  - Then: [Expected result]

- **Test 2**: [Description of what to test]
  - Given: [Setup]
  - When: [Action]
  - Then: [Expected result]

### Integration Tests

- **Test 1**: [Description of integration scenario]
  - Setup: [Test data/environment]
  - Action: [API call or interaction]
  - Verify: [Expected outcome]

### E2E Tests (if critical user flow)

- **Test Scenario**: [User flow description]
  - Steps:
    1. [Step 1]
    2. [Step 2]
    3. [Step 3]
  - Expected: [What user should see/experience]

---

## Definition of Done

- [ ] Code written following TDD approach (tests written first)
- [ ] All acceptance criteria scenarios pass
- [ ] Unit tests written and passing (>70% coverage for this feature)
- [ ] Integration tests written (if applicable)
- [ ] E2E test written (if critical flow)
- [ ] Code reviewed (if working with team)
- [ ] No linting errors (`nx lint`)
- [ ] No TypeScript errors (strict mode)
- [ ] No console errors in browser (frontend)
- [ ] API returns proper HTTP status codes (backend)
- [ ] Error handling displays user-friendly messages
- [ ] Loading states implemented where applicable
- [ ] Correlation IDs included in API calls
- [ ] Documentation updated (if needed)
- [ ] Feature deployed to dev environment
- [ ] Manual testing completed

---

## Implementation Notes

### Approach

[Describe the high-level approach to implementing this feature]

### Technical Decisions

- **Decision 1**: [What was decided and why]
- **Decision 2**: [What was decided and why]

### Potential Challenges

- **Challenge 1**: [Description]
  - **Solution**: [How to address it]

- **Challenge 2**: [Description]
  - **Solution**: [How to address it]

---

## Story Points Estimation

**Estimated Story Points**: [X]

**Breakdown**:
- **Analysis & Design**: [X hours]
- **Implementation**: [X hours]
- **Testing**: [X hours]
- **Documentation**: [X hours]
- **Buffer**: [X hours]

**Total Estimated Hours**: [X hours]

**Confidence Level**: [High/Medium/Low]

---

## Related Work

- **Blocks**: [Features that are blocked by this one]
- **Blocked By**: [Features that block this one]
- **Related To**: [Features that are related but not blocking]

---

**Document Version**: 1.0
**Last Updated**: [YYYY-MM-DD]
**Status**: Ready for Development
