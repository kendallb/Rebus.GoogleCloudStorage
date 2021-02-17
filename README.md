# Rebus.GoogleCloudStorage

[![install from nuget](https://img.shields.io/nuget/v/AMain.Rebus.GoogleCloudStorage.svg?style=flat-square)](https://www.nuget.org/packages/AMain.Rebus.GoogleCloudStorage)

Provides Google Cloud Storage bucket implementation for [Rebus](https://github.com/rebus-org/Rebus) of

* data bus storage
* subscription storage
* saga auditing snapshots

You can configure the bucket data bus like this:

```csharp
var storageClient = StorageClient.Create();

Configure.With(...)
	.(...)
	.DataBus(d => d.StoreInGoogleCloudStorage(storageClient, "my-project-id", "my-bucket"))
	.Start();
```

or to pass configure more options:

```csharp
var storageClient = StorageClient.Create();

Configure.With(...)
	.(...)
    .DataBus(d =>
    {
        var options = new GoogleCloudStorageDataBusOptions("my-project-id", "my-bucket")
        {
            DoNotUpdateLastReadTime = true,
            AutoCreateBucket = false,
            ObjectKeyPrefix = "my-prefix",
            ObjectKeySuffix = ".my-suffix",
        };
       d.StoreInGoogleCloudStorage(storageClient, options);
    })
	.Start();
```

You can configure the subscription store like this:

```csharp
var storageClient = StorageClient.Create();

Configure.With(...)
	.(...)
	.Subscriptions(d => d.StoreInGoogleCloudStorage(storageClient, "my-project-id", "my-bucket"))
	.Start();
```

or to pass configure more options:

```csharp
var storageClient = StorageClient.Create();

Configure.With(...)
	.(...)
    .Subscriptions(d =>
    {
        var options = new GoogleCloudStorageSubscriptionOptions("my-project-id", "my-bucket")
        {
            DoNotUpdateLastReadTime = true,
            AutoCreateBucket = false,
            ObjectKeyPrefix = "my-prefix-folder/",
        };
        d.StoreInGoogleCloudStorage(storageClient, options);
    })
	.Start();
```

You can configure the saga auditing storage like this:

```csharp
var storageClient = StorageClient.Create();

Configure.With(...)
	.(...)
    .Sagas(...)
    .Options(o => o.EnableSagaAuditing().StoreInGoogleCloudStorage(storageClient, "my-project-id", "my-bucket"))
	.Start();
```

or to pass configure more options:

```csharp
var storageClient = StorageClient.Create();

Configure.With(...)
	.(...)
    .Sagas(...)
    .Options(o =>
    {
        var options = new GoogleCloudStorageSagaSnapshotOptions("my-project-id", "my-bucket")
        {
            DoNotUpdateLastReadTime = true,
            AutoCreateBucket = false,
            ObjectKeyPrefix = "my-snapshots-folder/",
        };
        o.EnableSagaAuditing().StoreInGoogleCloudStorage(storageClient, options);
    })
	.Start();
```

![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---
