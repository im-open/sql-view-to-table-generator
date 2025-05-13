# sql-view-to-table-generator

This action is a replacement for the [Sql-View-to-Table-Creation](https://github.com/im-open/Sql-View-to-Table-Creation) repo. It wraps the tool in a GitHub Action for use in Workflows.

This GitHub Action will look at the views for a specified schema on a SQL Server instance and generate files for each with a `CREATE TABLE` statement. As such, a running SQL Server instance needs to be in place before this Action can be used (see the [Examples](#examples)).

The main use case for this Action is when you need to reference a view, but either can't or don't want to spin up a database with every dependency the view needs. This is particularly handy for local development and automated testing. You don't need to spin up an entire monolithic database just to run your tests. You can spin up something much more paired down and develop/test against that.

## Index<!-- omit in toc -->

- [sql-view-to-table-generator](#sql-view-to-table-generator)
  - [Inputs](#inputs)
  - [Usage Examples](#usage-examples)
  - [Contributing](#contributing)
    - [Incrementing the Version](#incrementing-the-version)
    - [Source Code Changes](#source-code-changes)
    - [Recompiling Manually](#recompiling-manually)
    - [Updating the README.md](#updating-the-readmemd)
  - [Code of Conduct](#code-of-conduct)
  - [License](#license)

## Inputs

| Parameter             | Is Required | Default   | Description                                                                                                                                                     |
|-----------------------|-------------|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
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

## Usage Examples

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
        uses: im-open/sql-view-to-table-generator@v1.1.0
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
        uses: im-open/sql-view-to-table-generator@v1.1.1
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

When creating PRs, please review the following guidelines:

- [ ] The action code does not contain sensitive information.
- [ ] At least one of the commit messages contains the appropriate `+semver:` keywords listed under [Incrementing the Version] for major and minor increments.
- [ ] The action has been recompiled.  See [Recompiling Manually] for details.
- [ ] The README.md has been updated with the latest version of the action.  See [Updating the README.md] for details.

### Incrementing the Version

This repo uses [git-version-lite] in its workflows to examine commit messages to determine whether to perform a major, minor or patch increment on merge if [source code] changes have been made.  The following table provides the fragment that should be included in a commit message to active different increment strategies.

| Increment Type | Commit Message Fragment                     |
|----------------|---------------------------------------------|
| major          | +semver:breaking                            |
| major          | +semver:major                               |
| minor          | +semver:feature                             |
| minor          | +semver:minor                               |
| patch          | *default increment type, no comment needed* |

### Source Code Changes

The files and directories that are considered source code are listed in the `files-with-code` and `dirs-with-code` arguments in both the [build-and-review-pr] and [increment-version-on-merge] workflows.  

If a PR contains source code changes, the README.md should be updated with the latest action version and the action should be recompiled.  The [build-and-review-pr] workflow will ensure these steps are performed when they are required.  The workflow will provide instructions for completing these steps if the PR Author does not initially complete them.

If a PR consists solely of non-source code changes like changes to the `README.md` or workflows under `./.github/workflows`, version updates and recompiles do not need to be performed.

### Recompiling Manually

This command utilizes [esbuild] to bundle the action and its dependencies into a single file located in the `dist` folder.  If changes are made to the action's [source code], the action must be recompiled by running the following command:

```sh
# Installs dependencies and bundles the code
npm run build
```

### Updating the README.md

If changes are made to the action's [source code], the [usage examples] section of this file should be updated with the next version of the action.  Each instance of this action should be updated.  This helps users know what the latest tag is without having to navigate to the Tags page of the repository.  See [Incrementing the Version] for details on how to determine what the next version will be or consult the first workflow run for the PR which will also calculate the next version.

## Code of Conduct

This project has adopted the [im-open's Code of Conduct](https://github.com/im-open/.github/blob/main/CODE_OF_CONDUCT.md).

## License

Copyright &copy; 2023, Extend Health, LLC. Code released under the [MIT license](LICENSE).

 <!-- Links -->
[Incrementing the Version]: #incrementing-the-version
[Recompiling Manually]: #recompiling-manually
[Updating the README.md]: #updating-the-readmemd
[source code]: #source-code-changes
[usage examples]: #usage-examples
[build-and-review-pr]: ./.github/workflows/build-and-review-pr.yml
[increment-version-on-merge]: ./.github/workflows/increment-version-on-merge.yml
[esbuild]: https://esbuild.github.io/getting-started/#bundling-for-node
[git-version-lite]: https://github.com/im-open/git-version-lite
