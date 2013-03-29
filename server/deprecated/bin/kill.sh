#!/bin/bash

ps -ef|awk '/bin\/java/ {print $2}' |xargs kill
