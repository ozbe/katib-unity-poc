# HP

PoC of running a Unity Linux Build with
[Katib](https://github.com/kubeflow/katib)

## Introduction

Katib is a Kubernetes-based system for Hyperparameter Tuning and Neural
Architecture Search. Katib supports a number of ML frameworks, including
TensorFlow, Apache MXNet, PyTorch, XGBoost, and others.

To support hyperparameter tuning, Katib has the following concepts:
- [Experiment](https://github.com/kubeflow/katib#experiment) -
  represents a single optimization run over a feasible space
  - Objective - the metric and goal being optimized
  - Search Space - constraints for parameters
  - Search Algorithm - find optimal configurations using random search,
    TPE, grid search, hyperband or Bayesian Optimization
- [Suggestion](https://github.com/kubeflow/katib#suggestion) - a
  proposed solution to the optimization problem
- [Trial](https://github.com/kubeflow/katib#trial) - one iteration of
  the optimization process, which is a worker job with a list oof
  parameter assignments from the corresponding suggestion
- [Worker Job](https://github.com/kubeflow/katib#trial) - the process
  for evaluating a Trial and calculating its objective value

The worker can be a Kubernetes job, as well as some specialized kinds.

For purposes of this project, we are going to focus on running a Unity
Linux Build as a Kubernetes Job that can be used as a worker job. The
job will take the inputs provided by the trial and output metrics that
can be used in evaluating the experiment.

## Prereqs

**Required**
- Vagrant
- Virtual Box
- kubectl

**Optional**
- Unity version 2017.4.25f1 w/ Linux build support
- Docker
- Docker Hub Repo

## MiniKF

From project root, run

```
vagrant up
```

If you want to start from scratch follow the
[MiniKF directions](https://www.kubeflow.org/docs/other-guides/virtual-dev/getting-started-minikf/).

## kubectl

Ensure MiniKF is running and open <https://10.10.10.10/>

Expand the `Use kubectl (advanced)` section at the bottom of the page.

Follow steps 1 & 2 and download `minikf-kubeconfig` to the project root.

Any commands related to `kubectl` will assume you are in the project
root and use the name `minikf-kubeconfig`.

To make sure everything is work, run a smoke test using
[random-example.yaml](https://www.kubeflow.org/docs/components/hyperparameter-tuning/hyperparameter/#example-using-random-algorithm)

## Unity

> **Note:** You can skip this section if you want to use the HP docker
> image published to Docker Hub.

Open HP Unity Project in Unity from the project root.

There is only one scene, Scene. Nothing much to see in the Scene view.

However, if you look in the Hierarchy Window, there is a GameObject
called `HP`.

The `HP` GameObject has a script attached titled
[HPScript](Assets/HPScript.cs).

[HPScript](Assets/HPScript.cs) reads in the parameters suggested by the
Trial as environment variables.

[HPScript](Assets/HPScript.cs) uses the parameters to determine a hash
to output as a metric, along with a time metric.

You don't need to change anything with the script, but you do need to
make a build in Unity.

In Unity, go to `File` > `Build Settings`

We are going to make a Linux Build

1. Select `PC, Mac & Linux Standalone`
2. `Target Platform`: `Linux`
3. `Architecture`: `x86_64`
4. `Development Build`: `□`
5. `Headless Mode`: `✓`
6. `Compression Method`: `Default`
7. Then press `Build` and choose the project root as the destination.

## Docker

> **Note:** You can skip this section if you want to use the HP docker
> image published to Docker Hub.

After the Unity Linux Build we have all the assets we need to make a
Docker image to run on MiniKF.

To start, lets build the Docker image. You can feel free to change

```
docker build -t hp:0.0.6 .
```

Now lets run the Docker image locally to make sure it is setup

```
docker run --rm \
   -v `pwd`/HP_Data:/HP_Data \
   -v `pwd`/HP.x86_64:/HP.x86_64 \
   -e weapon0=weapon0.d \
   -e weapon1=weapon1.b \
   -e weapon2=weapon2.a \
   -e weapon3=weapon3.a \
   -e weapon4=weapon4.a \
   --name hp \
   hp:0.0.6
```

You shouldn't see any errors and if you check out
`HP_Data/metrics/output.txt` you should see the metrics output similar
to

```
weapon3=weapon3.a
weapon4=weapon4.a
weapon0=weapon0.d
weapon1=weapon1.b
weapon2=weapon2.a
deaths=0.811270373363809
time=637206083666253880
```

I didn't get around to determining if it was feasible to connect MiniKF
to my local docker.

Instead, I published the image to a public Docker Hub repo,
[ozbe/hp](https://hub.docker.com/r/ozbe/hp).

You can use whatever repo you want to iterate on your own.

With the repo setup, lets tag and push the build we just made

```
docker tag hp:0.0.6 ozbe/hp:0.0.6
docker push ozbe/hp:0.0.6
```

## Trial Template

Katib has the concept of a Trial Template that can be pretty handy if
you want to run experiments with similar trial configuration (otherwise
you can define the trial config in the experiment).

Katib comes with an example and for our purposes we need to add another,
`hpTemplate.yaml`.

`hpTemplate.yaml` has the Kubernetes Job definiton that we will need for
our experiment. Note that it should point to the Docker Image you want
to use (I call that out if you did the Unity and Docker steps above).

[trial-template.yaml](trial-template.yaml) is located at the project
root. It documents the `hptemplate.yaml` and can be applied if you don't
have any other Trial templates you are attached to, other than
`defaultTrialTemplate.yaml`.

> **WARNING:** Do not blindly run this command. Read above first!

```
kubectl --kubeconfig minikf-kubeconfig apply -f trial-template.yaml -n kubeflow-user
```

## Experiment

Finally, we are to the experiment.

You can see the experiment configured in
[hp-experiment.yaml](hp-experiment.yaml).

Right now it is configured to use Grid Search.

To create the experiment, run the following

```
kubectl --kubeconfig minikf-kubeconfig apply -f hp-experiment.yaml -n kubeflow-user
```

## Katib UI

With the HP Experiment running (though it isn't required), navigate to
<http://10.10.10.10:8080/_/katib/?ns=kubeflow-user> to see the
experiment.

If you choose the experiment, continue to refresh to see the progress.

Katib is running using the Grid Search algorithm to test the different
parameter combinations.

Try one of the other algorithms and see how the results differ.

## FAQ

### Kubeflow is asking me for username and password

Go to <http://10.10.10.10/> and look at the Credentials square.
