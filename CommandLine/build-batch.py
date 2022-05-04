import argparse
import subprocess
import platform

class Build:
    pass
build = Build()

parser = argparse.ArgumentParser(description='Build Unity Script for Jenkins')
parser.add_argument('-s', '--setup', help='Build Setup File Path ex)BuildSetup.asset', required=True)
parser.add_argument('-e', '--entry', help='Build Setup Entry Name', required=True)
parser.add_argument('-b', '--build', help='Build Number', required=True)
parser.add_argument('-p', '--project', help='Unity Proejct Path (Directory)', required=True)
parser.add_argument('-u', '--unity', help='Unity Engine Path (Directory)', required=True)
parser.add_argument('-i', '--serial', help='Unity Pro Serial')
parser.parse_args(namespace=build)

executes = ["-quit", "-batchmode" ,"-nographics" ,"-projectPath", build.project, "-executeMethod" ,"Soma.Build.BuildProcess.BuildWithArgs" ,"-buildSetupPath", build.setup, "-buildNumber" ,build.build, "-buildEntryName", build.entry]
if build.serial != None:
    executes = executes + ["-serial", build.serial]
if "macOS" in platform.platform():
    executes = ["open", "-a"] + [build.unity, "--args"] + executes
else :
    executes = [build.unity] + executes

print("Run : ", " ".join(executes))
p = subprocess.Popen(executes,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.STDOUT,
                    universal_newlines=True)
                     
while p.poll() == None:
	out = p.stdout.readline()
	print(out, end='')