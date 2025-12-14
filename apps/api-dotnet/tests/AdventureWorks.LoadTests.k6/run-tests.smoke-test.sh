#!/usr/bin/env bash
#
# Dependency-free smoke test for run-tests.sh's LOADTEST_* fail-fast/opt-in logic. No
# bats-core/shellspec exists anywhere in this repo, so this exercises the two paths that don't
# require a real Entra tenant by stubbing `k6` on PATH and asserting exit codes / stderr / whether
# the stub was invoked.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
STUB_DIR="$(mktemp -d)"
INVOKED_MARKER="${STUB_DIR}/k6-invoked"

cleanup() {
  rm -rf "${STUB_DIR}"
}
trap cleanup EXIT

cat > "${STUB_DIR}/k6" <<EOF
#!/usr/bin/env bash
touch "${INVOKED_MARKER}"
exit 0
EOF
chmod +x "${STUB_DIR}/k6"

failures=0

run_case() {
  local description="$1"
  shift
  echo "-- ${description}"
}

assert_eq() {
  local expected="$1" actual="$2" message="$3"
  if [[ "${expected}" != "${actual}" ]]; then
    echo "FAIL: ${message} (expected '${expected}', got '${actual}')"
    failures=$((failures + 1))
  else
    echo "PASS: ${message}"
  fi
}

assert_contains() {
  local haystack="$1" needle="$2" message="$3"
  if [[ "${haystack}" != *"${needle}"* ]]; then
    echo "FAIL: ${message} (expected output to contain '${needle}')"
    echo "  actual output: ${haystack}"
    failures=$((failures + 1))
  else
    echo "PASS: ${message}"
  fi
}

# --- Scenario 1: one var set, four missing, profile=load -> fail fast, k6 never invoked ---
run_case "profile=load, only LOADTEST_TENANT_ID set"
rm -f "${INVOKED_MARKER}"
set +e
stderr_output="$(
  PATH="${STUB_DIR}:${PATH}" \
  LOADTEST_TENANT_ID="tenant-id" \
  LOADTEST_CLIENT_ID="" \
  LOADTEST_API_SCOPE="" \
  LOADTEST_USERNAME="" \
  LOADTEST_PASSWORD="" \
  "${SCRIPT_DIR}/run-tests.sh" load 2>&1 1>/dev/null
)"
exit_code=$?
set -e

assert_eq "1" "${exit_code}" "load profile with 4 missing vars exits non-zero"
assert_contains "${stderr_output}" "LOADTEST_CLIENT_ID" "error names LOADTEST_CLIENT_ID"
assert_contains "${stderr_output}" "LOADTEST_API_SCOPE" "error names LOADTEST_API_SCOPE"
assert_contains "${stderr_output}" "LOADTEST_USERNAME" "error names LOADTEST_USERNAME"
assert_contains "${stderr_output}" "LOADTEST_PASSWORD" "error names LOADTEST_PASSWORD"
if [[ -f "${INVOKED_MARKER}" ]]; then
  echo "FAIL: stub k6 should never have been invoked"
  failures=$((failures + 1))
else
  echo "PASS: stub k6 was never invoked"
fi

# --- Scenario 2: profile=smoke, zero LOADTEST_* vars set -> runs unauthenticated ---
run_case "profile=smoke, zero LOADTEST_* vars set"
rm -f "${INVOKED_MARKER}"
set +e
env -u LOADTEST_TENANT_ID -u LOADTEST_CLIENT_ID -u LOADTEST_API_SCOPE -u LOADTEST_USERNAME -u LOADTEST_PASSWORD \
  PATH="${STUB_DIR}:${PATH}" \
  "${SCRIPT_DIR}/run-tests.sh" smoke >/dev/null 2>&1
exit_code=$?
set -e

assert_eq "0" "${exit_code}" "smoke profile with no LOADTEST_* vars exits zero"
if [[ -f "${INVOKED_MARKER}" ]]; then
  echo "PASS: stub k6 was invoked"
else
  echo "FAIL: stub k6 should have been invoked"
  failures=$((failures + 1))
fi

# --- Scenario 3: one var set, four missing, profile=smoke-human-resources -> fail fast, k6 never invoked ---
run_case "profile=smoke-human-resources, only LOADTEST_TENANT_ID set"
rm -f "${INVOKED_MARKER}"
set +e
stderr_output="$(
  PATH="${STUB_DIR}:${PATH}" \
  LOADTEST_TENANT_ID="tenant-id" \
  LOADTEST_CLIENT_ID="" \
  LOADTEST_API_SCOPE="" \
  LOADTEST_USERNAME="" \
  LOADTEST_PASSWORD="" \
  "${SCRIPT_DIR}/run-tests.sh" smoke-human-resources 2>&1 1>/dev/null
)"
exit_code=$?
set -e

assert_eq "1" "${exit_code}" "smoke-human-resources profile with 4 missing vars exits non-zero"
assert_contains "${stderr_output}" "LOADTEST_CLIENT_ID" "error names LOADTEST_CLIENT_ID (smoke-human-resources)"
assert_contains "${stderr_output}" "LOADTEST_API_SCOPE" "error names LOADTEST_API_SCOPE (smoke-human-resources)"
assert_contains "${stderr_output}" "LOADTEST_USERNAME" "error names LOADTEST_USERNAME (smoke-human-resources)"
assert_contains "${stderr_output}" "LOADTEST_PASSWORD" "error names LOADTEST_PASSWORD (smoke-human-resources)"
if [[ -f "${INVOKED_MARKER}" ]]; then
  echo "FAIL: stub k6 should never have been invoked (smoke-human-resources)"
  failures=$((failures + 1))
else
  echo "PASS: stub k6 was never invoked (smoke-human-resources)"
fi

# --- Scenario 4: one var set, four missing, profile=smoke-person -> fail fast, k6 never invoked ---
run_case "profile=smoke-person, only LOADTEST_TENANT_ID set"
rm -f "${INVOKED_MARKER}"
set +e
stderr_output="$(
  PATH="${STUB_DIR}:${PATH}" \
  LOADTEST_TENANT_ID="tenant-id" \
  LOADTEST_CLIENT_ID="" \
  LOADTEST_API_SCOPE="" \
  LOADTEST_USERNAME="" \
  LOADTEST_PASSWORD="" \
  "${SCRIPT_DIR}/run-tests.sh" smoke-person 2>&1 1>/dev/null
)"
exit_code=$?
set -e

assert_eq "1" "${exit_code}" "smoke-person profile with 4 missing vars exits non-zero"
assert_contains "${stderr_output}" "LOADTEST_CLIENT_ID" "error names LOADTEST_CLIENT_ID (smoke-person)"
assert_contains "${stderr_output}" "LOADTEST_API_SCOPE" "error names LOADTEST_API_SCOPE (smoke-person)"
assert_contains "${stderr_output}" "LOADTEST_USERNAME" "error names LOADTEST_USERNAME (smoke-person)"
assert_contains "${stderr_output}" "LOADTEST_PASSWORD" "error names LOADTEST_PASSWORD (smoke-person)"
if [[ -f "${INVOKED_MARKER}" ]]; then
  echo "FAIL: stub k6 should never have been invoked (smoke-person)"
  failures=$((failures + 1))
else
  echo "PASS: stub k6 was never invoked (smoke-person)"
fi

echo
if [[ "${failures}" -eq 0 ]]; then
  echo "All checks passed."
  exit 0
else
  echo "${failures} check(s) failed."
  exit 1
fi
