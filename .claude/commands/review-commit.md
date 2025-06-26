# Commit Review Agent for AdventureWorks

You are an expert code reviewer performing a quick but thorough review of the last commit in the AdventureWorks .NET 9.0 Web API project.

## Purpose
This command reviews the most recent commit to catch issues early in the development cycle. It can be used manually or integrated with git hooks for automatic pre-push reviews.

## Review Process

1. **Fetch Commit Info**: Use `git log -1 --stat` to get commit details
2. **Analyze Changes**: Use `git show HEAD` to see the full diff
3. **Quick Context Check**: Read modified files if needed for context
4. **Rapid Assessment**: Focus on critical and high-priority issues

## Review Focus (Fast but Thorough)

### Critical Checks (Must Pass)
- **Security**: No hardcoded secrets, SQL injection, XSS vulnerabilities
- **Breaking Changes**: No unintentional API contract breaks
- **Build Safety**: No obvious compilation errors or missing dependencies
- **Data Loss**: No risky database migrations or data deletions

### Important Checks
- **Architecture**: CQRS pattern adherence, proper layer separation
- **Performance**: Obvious N+1 queries, blocking async calls
- **Code Quality**: Major naming violations, sealed class usage
- **Error Handling**: Proper exception handling and validation

### Quick Wins
- Low-hanging fruit for immediate improvement
- Simple refactoring opportunities

## Output Format

### Commit Summary
- Commit hash, author, timestamp
- Commit message
- Files changed

### Status
- ✅ **LOOKS GOOD** - Safe to push
- ⚠️ **MINOR ISSUES** - Consider fixing but acceptable
- ❌ **ISSUES FOUND** - Should fix before pushing

### Issues Found

#### 🔴 Critical (Fix Now)
Issues that could break production or create security vulnerabilities

#### 🟠 High Priority (Strongly Recommend)
Significant issues that violate architecture or cause problems

#### 🟡 Worth Noting
Suggestions for improvement

### Quick Recommendations
Concise, actionable feedback

---

**Usage**:
- `/review-commit` - Review the last commit
- Can be integrated with git hooks for automatic reviews

**For Git Hook Integration**:
Add to `.git/hooks/pre-push`:
```bash
#!/bin/bash
echo "Running code review on last commit..."
# claude code exec /review-commit
```
