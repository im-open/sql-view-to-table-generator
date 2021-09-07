# sql-view-to-table-generator

This action is a replacement for the [Sql-View-to-Table-Creation](https://github.com/im-open/Sql-View-to-Table-Creation) repo. It wraps the tool in a GitHub Action for use in Workflows.

This GitHub Action will look at the views for a specified schema on a SQL Server instance and generate files for each with a `CREATE TABLE` statement. As such, a running SQL Server instance needs to be in place before this Action can be used (see the [Examples](#Examples)).

The main use case for this Action is when you need to reference a view, but either can't or don't want to spin up a database with every dependency the view needs. This is particularly handy for local development and automated testing. You don't need to spin up an entire monolithic database just to run your tests. You can spin up something much more paired down and develop/test against that.
    

## Inputs
| Parameter             | Is Required | Default    | Description           |
| --------------------- | ----------- | ---------- | --------------------- |
| `schema-names`        | true        | N/A        | A comma separated list of schemas that hold the views that are to have create table scripts created for them. |
| `db-name`             | true        | N/A        | The name of the database to use. |
| `db-server`           | true        | localhost  | The name of the database server to use. |
| `db-port`             | false       | 1433       | The name of the database server port to use. |
| `default-branch`      | true        | main       | The name of the default branch of the running repo. |
| `nuget-retrieval-url` | true        | N/A        | The url where the packages to compare against the generated files can be found. |
| `publish-packages`    | false       | false      | A flag determining whether or not to publish nuget packages with the create table files. |
| `nuget-publish-url`   | false       | N/A        | The url where the generated packages will be published. This should be set if the `publish-packages` flag is `true`, otherwise an error will occur. |
| `nuget-api-key`       | false       | N/A        | The API key for the `nuget-publish-url`. If that url is set, this should be too. |
| `nuget-folder`        | false       | N/A        | The name of the folder to put the nuget files that will be compared to the generated create table files. Defaults to `./.build/.nuget` in this action's folder. |
| `package-folder`      | false       | N/A | The name of the folder where the generated create table files will go. Defaults to './.build/.packages' in this action's folder. |

## Examples

**Without publishing the views to a nuget repository**
```yml
jobs:
  job1:
    runs-on: [self-hosted]
    steps:
      - uses: actions/checkout@v2

      - name: Install Flyway
        uses: im-open/setup-flyway@v1.0.0
        with:
          version: 7.2.0

      - name: Build Database
        uses: im-open/build-database-ci-action@v1.0.1
        with:
          db-server-name: localhost
          db-name: LocalDB
          drop-db-after-build: false

      - name: Create Views From Tables
        uses: im-open/sql-view-to-table-generator@v1.0.0
        with:
          schema-names: "dbo,MySchema"
          db-name: LocalDb
          db-server: localhost
          nuget-retrieval-url: https://www.nuget.org/
```

**Publishing the views to a nuget repository**
```yml
jobs:
  job1:
    runs-on: [self-hosted]
    steps:
      - uses: actions/checkout@v2

      - name: Install Flyway
        uses: im-open/setup-flyway@v1.0.0
        with:
          version: 7.2.0

      - name: Build Database
        uses: im-open/build-database-ci-action@v1.0.4
        with:
          db-server-name: localhost
          db-name: LocalDB
          drop-db-after-build: false

      - name: Authenticate with GitHub Packages on Windows
        uses: im-open/authenticate-with-gh-packages-for-nuget@v1.0.0
        with:
          github-token: ${{ secrets.MY_GH_PACKAGES_ACCESS_TOKEN }} # Token has read:packages scope and is authorized for each of the orgs
          orgs: 'my-org'

      - name: Create Views From Tables
        uses: im-open/sql-view-to-table-generator@v1.0.1
        with:
          schema-names: dbo
          db-name: LocalDb
          db-server: localhost
          nuget-retrieval-url: "https://nuget.pkg.github.com/my-org/index.json" # A GitHub packages url for my-org
          publish-packages: true
          nuget-publish-url: "https://github.com/my-org/my-repo" # The url to publish packages to
          nuget-api-key: "${{ secrets.MY_GH_PACKAGES_ACCESS_TOKEN }}" # A token that has access to publish packages
```

## Code of Conduct

This project has adopted the [im-open's Code of Conduct](https://github.com/im-open/.github/blob/master/CODE_OF_CONDUCT.md).

## License

Copyright &copy; 2021, Extend Health, LLC. Code released under the [MIT license](LICENSE).
