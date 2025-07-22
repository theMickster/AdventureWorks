/** Authenticated user profile extracted from Entra ID token claims. */
export interface AuthUser {
  /** Display name from the id_token 'name' claim. */
  name: string;
  /** Email address (preferred_username or mail claim). */
  email: string;
  /** Entra ID Object ID (oid claim). */
  oid: string;
  /** User Principal Name (UPN). */
  username: string;
}
