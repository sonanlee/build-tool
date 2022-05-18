def defaultDeploy = "Win64"
def allJob = currentBuild.fullDisplayName.tokenize('/') as String[];
MAIN_PROJECT_NAME = allJob.first();
def allTokens = MAIN_PROJECT_NAME.tokenize('-') as String[];
PLATFORM = allTokens.last();

pipeline {
  agent any
  parameters {
      choice (choices:['Addressable-CCD-BuildContent','Addressable-CCD-Rebuild'], name:'entryName', description:"Choose Build Entry")
      string(name:'RequestUser', defaultValue:'SomaButler', description:'Build를 요청한 User를 넣어주세요.')
  }
  environment {
      MY_PROJECT_NAME = "${JOB_NAME}"
  }
  stages {
    stage('error') {
      steps {
        echo "0 : ${JOB_NAME}"
        echo "A : ${JOB_BASE_NAME}"
        echo "B : ${params.entryName}"
        echo "C : ${currentBuild.projectName}"
        echo "C : ${currentBuild.fullProjectName}"
        echo "D : ${currentBuild.displayName}"
        echo "F : ${currentBuild.fullDisplayName}"
        echo "H : ${MAIN_PROJECT_NAME}"
        echo "I : ${PLATFORM}"
        echo "I : ${currentBuild.changeSets.first().getKind()}"
        echo "I : ${currentBuild.changeSets.last().getKind()}"
        echo "I : ${GIT_COMMIT}"
        echo "I : ${params.RequestUser}"
      }
    }
    stage ('if'){
      steps {
        script {
          if (MY_PROJECT_NAME == JOB_NAME)
          {
            echo("hello")
          }
          else
          {
            echo("bye")
          }
        }
      }
    }

  }
}