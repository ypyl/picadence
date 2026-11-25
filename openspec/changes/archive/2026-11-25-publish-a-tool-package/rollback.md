# Rollback

## What a rollback is here

A published package version can never be reused. nuget.org allows deleting a version
within 72 hours of publishing it; after that a version can only be unlisted, which hides
it from search and from a versionless install but keeps it for anyone who asks for that
exact version. Either way the number is spent: 0.4.0 can never be published again.

So the rollback has two halves: unlist the bad version so nobody installs it by default,
and ship the next version with the fix. The unlisted version is not repaired.

## The command

```
gh workflow run release.yml -f mode=unlist -f version=<semver>
```

It runs the `unlist` job in `release.yml`, which takes a one-hour key through trusted
publishing and runs

```
dotnet nuget delete Ypyl.Picadence <semver> --source https://api.nuget.org/v3/index.json
```

Watched with the Deploy line, because it is the same workflow.

This needs the trusted publishing policy to cover **Unlist or relist package versions**.
It does, and the policy also has the glob `Ypyl.Picadence`, so a workflow cannot touch
another package on the account.

## The executed part

The rehearsal feed was emptied and the install was tried again, which is what an
unlisted version looks like to a user:

```
$ dotnet tool install --tool-path ./rb --add-source ./nupkg Ypyl.Picadence
ypyl.picadence is not found in NuGet feeds https://api.nuget.org/v3/index.json,
C:\Users\ypyl\projects\trayclaw\.tmp\rehearsal\nupkg.
exit 1
```

Asking for the version explicitly fails too: `Version 0.4.0 of package ypyl.picadence is
not found in NuGet feeds ...`. The package was then put back and the install worked
again, so the removal is the whole difference.

## What is left to run at the first release

The production half: a real version, unlisted on nuget.org, and the next version
carrying the fix. That is watched on the day, because a rehearsal cannot unlist a
package that does not exist yet.
