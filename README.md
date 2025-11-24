<hr />

# WebApi

<h2>Creating migrations</h2>

1. To create a migration, open project folder. In the terminal cmd, type command '<code>pushd finance-tracker-app</code>'.

2. The next step is to delete the old migrations (if it has the same name of yours), if any, and that is the folder '<b>Migrations</b>' and enter:
'<code>
dotnet ef migrations add Initial --startup-project src/Deed/Deed.Api --project src/Deed/Deed.Infrastructure --output-dir Persistence/Migrations
</code>', instead of '<b>Initial</b>' specify the desired name. After that, a new folder '<b>Migrations</b>' will appear with the current date and a snapshot of the database.

- Backend > fly.io
- Frontend > netlify
- Database > FreeAspNetHosting

# Tests

<h2>Unit test</h2>

# Web UI

- For local development use <code>ng serve --configuration=development</code>