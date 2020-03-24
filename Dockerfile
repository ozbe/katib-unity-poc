# Inspired by Unity ml-agents
#   base and dependencies - https://github.com/Unity-Technologies/ml-agents/blob/master/Dockerfile
#   cmd - https://github.com/Unity-Technologies/ml-agents/blob/12cd89237e91963d0867bff46be581a36a7048f9/ml-agents-envs/mlagents_envs/environment.py#L265

FROM ubuntu:16.04

RUN apt-get update && apt-get -y upgrade
RUN apt-get install -y xvfb

# TODO - cleanup
# RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Unity Linux build output
COPY HP.x86_64 /HP.x86_64
COPY HP_Data /HP_Data

# Run Unity Linux build in xvfb
CMD xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /HP.x86_64