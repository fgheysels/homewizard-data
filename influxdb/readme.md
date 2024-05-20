## Deploy InfluxDB

Apply the `deployment.yaml` file that can be found in the `./influxdb/deploy` folder.  This deployment will deploy influxdb in the `influxdb` namespace.

Deploy using this command:

```
kubectl apply -f deployment.yaml
```

### Create database

Log in to the influxdb pod:

```
kubectl exec -n influxdb --stdin --tty influxdb-0 -- /bin/bash
```

After the above command, you're in the bash shell inside the influxdb container.
Enter the influx command shell by executing `influx`.
At the `influx` command shell, execute the following command to create a database:

```
> CREATE DATABASE "home_energy" WITH DURATION 10000d
```

Verify if the database is now availabe via this command:

```
show databases
```

> Ideally, this is done via init-scripts