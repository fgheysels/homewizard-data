## Deploy InfluxDB

Apply the `deployment.yaml` file that can be found in the `./influxdb/deploy` folder.  This deployment will deploy influxdb in the `influxdb` namespace.

Deploy using this command:

```
kubectl apply -f deployment.yaml
```

### Restore a backup (do this before deploying influxdb)

Copy the backup tar file using scp to the Raspberry pi

Extract the tar file

Deploy influxdb and verify if the data is available:

Login to the influxdb pod:

```
kubectl exec -n influxdb --stdin --tty influxdb-0 -- /bin/bash
```

Verify if the database is present:
```
influx
show databases
use <databasename>
show measurements
SELECT * FROM <measurement> LIMIT 10
```

### Create database (not necessary when a backup is restored)

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

### Stop / re-start Influxdb stateless service

Execute this command to (temporarily) stop a stateless service like InfluxdDb:

```
kubectl scale statefulset influxdb --replicas=0 -n influxdb
```

Or, the entire deployment:

```
kubectl scale deployment <deploymentname> --replicas=0 -n influxdb
```

### Backup influxdb data

- First stop Influxdb by scaling down the stateless-service or deployment
- archive the contents of the directory where the data is stored:
 
  ```
  sudo tar czf /home/pi/influxdb-backup.tar.gz <directory>
  ```

- copy the backup to another system.
  Use `scp` from another computer to 'grab' and copy the backup file that resides on the Raspberry Pi to copy it to the computer you're copying from:

  ```
  scp pi@<pi-ip>:/home/pi/influxdb-backup.tar.gz .
  ```