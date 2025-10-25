Define Custom Identity Models (POCOs):

Create your plain C# entities: User (with UserId,UserName,FirstName,LastName, Email, PasswordHash,PasswordSalt etc.), Role, Permission, and Platform.

The User entity must have a field for the hashed password.

Define your relationships:

User to Role (many-to-many).

Role to Permission (many-to-many).

Platform to Permission (one-to-many).

Add DbSets for these models to your ApplicationDbContext.

Implement Custom User & Password Logic:

Choose a strong hashing algorithm (e.g., BCrypt.Net or PasswordHasher<T> from Microsoft.AspNetCore.Identity.Core).

Create a PasswordHashingService to hash passwords on registration and verify them on login.

Create a UserService that handles finding a user by email/username and validating their password using the hashing service.

Integrate Duende IdentityServer (Custom):

In your Program.cs, configure Duende IdentityServer (.AddIdentityServer()).

Do not add .AddAspNetIdentity<>().

Instead, you must implement and register two key Duende interfaces:

IResourceOwnerPasswordValidator: Create a CustomPasswordValidator class. This class will use your UserService to validate a user's username and password (for the "password" grant type). On success, it returns a GrantValidationResult with the user's Id as the SubjectId.

IProfileService: This is the most important part. Create a CustomProfileService class. Its GetProfileDataAsync method will:

Receive the SubjectId (your user's Id).

Load the User from your database.

Load their Roles.

Get the ClientId from the context and find the corresponding Platform.

Load the Permissions for those roles that belong to that Platform.

Add all relevant user data (email, name, roles, permissions) as claims to context.IssuedClaims.

Create Custom Authentication Endpoints:

Since you aren't using Identity's SignInManager, you must create your own AuthController with a Login endpoint.

This endpoint will:

Take email and password from the request body.

Use your UserService to validate the credentials.

If valid, manually sign the user in using HttpContext.SignInAsync() with the IdentityServerConstants.DefaultCookieAuthenticationScheme. This creates the SSO session cookie that Duende will use.

Create a Logout endpoint that calls HttpContext.SignOutAsync().

Build the Admin Management API:

Create a secure Web API (e.g., /api/admin/*) protected by an "admin" policy.

Implement CRUD endpoints for:

Users

Roles

Platforms (This should also programmatically create/update the Duende Client configuration).

Permissions

Create endpoints for managing the relationships:

Assign/remove roles from a user.

Assign/remove permissions from a role.