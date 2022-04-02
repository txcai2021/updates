import os
import shutil
import subprocess

# ======================================================================================================================


def run_simulator(dir_run_command):

    simulator_cmd = '/run_simulator.cmd'
    run_command_file = dir_run_command + simulator_cmd
    proc = subprocess.Popen([run_command_file], shell=True, close_fds=True)
    stdout, stderr = proc.communicate()
    print('Run complete')
