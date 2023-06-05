# sql-view-to-table-generator

This action is a replacement for the [Sql-View-to-Table-Creation](https://github.com/im-open/Sql-View-to-Table-Creation) repo. It wraps the tool in a GitHub Action for use in Workflows.

This GitHub Action will look at the views for a specified schema on a SQL Server instance and generate files for each with a `CREATE TABLE` statement. As such, a running SQL Server instance needs to be in place before this Action can be used (see the [Examples](#examples)).

The main use case for this Action is when you need to reference a view, but either can't or don't want to spin up a database with every dependency the view needs. This is particularly handy for local development and automated testing. You don't need to spin up an entire monolithic database just to run your tests. You can spin up something much more paired down and develop/test against that.

## Index

- [sql-view-to-table-generator](#sql-view-to-table-generator)
  - [Index](#index)
  - [Inputs](#inputs)
  - [Examples](#examples)
  - [Contributing](#contributing)
    - [Recompiling Manually](#recompiling-manually)
    - [Incrementing the Version](#incrementing-the-version)
  - [Code of Conduct](#code-of-conduct)
  - [License](#license)

## Inputs

| Parameter             | Is Required | Default   | Description                                                                                                                                                     |
| --------------------- | ----------- | --------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `schema-names`        | true        | N/A       | A comma separated list of schemas that hold the views that are to have create table scripts created for them.                                                   |
| `db-name`             | true        | N/A       | The name of the database to use.                                                                                                                                |
| `db-server`           | true        | localhost | The name of the database server to use.                                                                                                                         |
| `db-port`             | false       | 1433      | The name of the database server port to use.                                                                                                                    |
| `default-branch`      | true        | main      | The name of the default branch of the running repo.                                                                                                             |
| `nuget-retrieval-url` | true        | N/A       | The url where the packages to compare against the generated files can be found.                                                                                 |
| `publish-packages`    | false       | false     | A flag determining whether or not to publish nuget packages with the create table files.                                                                        |
| `nuget-publish-url`   | false       | N/A       | The url where the generated packages will be published. This should be set if the `publish-packages` flag is `true`, otherwise an error will occur.             |
| `nuget-api-key`       | false       | N/A       | The API key for the `nuget-publish-url`. If that url is set, this should be too.                                                                                |
| `nuget-folder`        | false       | N/A       | The name of the folder to put the nuget files that will be compared to the generated create table files. Defaults to `./.build/.nuget` in this action's folder. |
| `package-folder`      | false       | N/A       | The name of the folder where the generated create table files will go. Defaults to './.build/.packages' in this action's folder.                                |
| `repository-url`      | false       | N/A       | Use when publishing to GitHub Packages. The url to the repository which should house the published packages.                                                    |

## Examples

**Without publishing the views to a nuget repository**

```yml
jobs:
  job1:
    runs-on: [self-hosted]
    steps:
      - uses: actions/checkout@v3

      - name: Install Flyway
        uses: im-open/setup-flyway@v1
        with:
          version: 7.2.0

      - name: Build Database
        uses: im-open/build-database-ci-action@v3
        with:
          db-server-name: localhost
          db-name: LocalDB
          drop-db-after-build: false

      - name: Create Views From Tables
        # You may also reference the major or major.minor version
        uses: im-open/sql-view-to-table-generator@v1.0.8
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
      - uses: actions/checkout@v3

      - name: Install Flyway
        uses: im-open/setup-flyway@v1
        with:
          version: 7.2.0

      - name: Build Database
        uses: im-open/build-database-ci-action@v3
        with:
          db-server-name: localhost
          db-name: LocalDB
          drop-db-after-build: false

      - name: Authenticate with GitHub Packages on Windows
        uses: im-open/authenticate-with-gh-package-registries@v1
        with:
          github-token: ${{ secrets.MY_GH_PACKAGES_ACCESS_TOKEN }} # Token has read:packages scope and is authorized for each of the orgs
          orgs: 'my-org'

      - name: Create Views From Tables
        # You may also reference the major or major.minor version
        uses: im-open/sql-view-to-table-generator@v1.0.8
        with:
          schema-names: dbo
          db-name: LocalDb
          db-server: localhost
          nuget-retrieval-url: "https://nuget.pkg.github.com/my-org/index.json" # A GitHub packages url for my-org
          publish-packages: true
          nuget-publish-url: "https://github.com/my-org/my-repo" # The url to publish packages to
          nuget-api-key: "${{ secrets.MY_GH_PACKAGES_ACCESS_TOKEN }}" # A token that has access to publish packages
          repository-url: "git://github.com/my-org/my-repo.git" # The URL to the repository.
```

## Contributing

When creating new PRs please ensure:

1. For major or minor changes, at least one of the commit messages contains the appropriate `+semver:` keywords listed under [Incrementing the Version](#incrementing-the-version).
1. The action code does not contain sensitive information.

When a pull request is created and there are changes to code-specific files and folders, the build workflow will run and it will recompile the action and push a commit to the branch if the PR author has not done so. The usage examples in the README.md will also be updated with the next version if they have not been updated manually. The following files and folders contain action code and will trigger the automatic updates:

- `action.yml`
- `package.json`
- `package-lock.json`
- `src/**`
- `.build-linux/**`
- `.build-win/**`

There may be some instances where the bot does not have permission to push changes back to the branch though so these steps should be done manually for those branches. See [Recompiling Manually](#recompiling-manually) and [Incrementing the Version](#incrementing-the-version) for more details.

### Recompiling Manually

If changes are made to the action's code in this repository, or its dependencies, the action can be re-compiled by running the following command:

```sh
# Installs dependencies and bundles the code
npm run build

# Bundle the code (if dependencies are already installed)
npm run bundle
```

These commands utilize [esbuild](https://esbuild.github.io/getting-started/#bundling-for-node) to bundle the action and
its dependencies into a single file located in the `dist` folder.

### Incrementing the Version

Both the build and PR merge workflows will use the strategies below to determine what the next version will be.  If the build workflow was not able to automatically update the README.md action examples with the next version, the README.md should be updated manually as part of the PR using that calculated version.

This action uses [git-version-lite] to examine commit messages to determine whether to perform a major, minor or patch increment on merge. The following table provides the fragment that should be included in a commit message to active different increment strategies.
| Increment Type | Commit Message Fragment                     |
| -------------- | ------------------------------------------- |
| major          | +semver:breaking                            |
| major          | +semver:major                               |
| minor          | +semver:feature                             |
| minor          | +semver:minor                               |
| patch          | _default increment type, no comment needed_ |

## Code of Conduct

This project has adopted the [im-open's Code of Conduct](https://github.com/im-open/.github/blob/master/CODE_OF_CONDUCT.md).

## License

Copyright &copy; 2021, Extend Health, LLC. Code released under the [MIT license](LICENSE).

[git-version-lite]: https://github.com/im-open/git-version-lite
