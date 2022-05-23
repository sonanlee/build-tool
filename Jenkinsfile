def defaultDeploy = "Win64"
def allJob = currentBuild.fullDisplayName.tokenize('/') as String[];
MAIN_PROJECT_NAME = allJob.first();
def allTokens = MAIN_PROJECT_NAME.tokenize('-') as String[];
PLATFORM = allTokens.last();

pipeline {
  agent any
  parameters {
      booleanParam(name: 'Refresh', defaultValue: true, description: 'Do you want to build?')
      booleanParam(name: 'CleanBuild', defaultValue: false, description: 'Is Clean Build? (Remove previous build libraries)')
      choice (choices:['Addressable-CCD-BuildContent','Addressable-CCD-Rebuild'], name:'entryName', description:"Choose Build Entry")
      string(name:'RequestUser', defaultValue:'SomaButler', description:'Build를 요청한 User를 넣어주세요.')
  }
  options {
    disableConcurrentBuilds abortPrevious: false
    skipDefaultCheckout params.Refresh
  }
  environment {
      MY_PROJECT_NAME = "${JOB_NAME}"
  }
  stages {
    stage('only-script'){
      when {
        expression { params.Refresh == true }
        expression { params.CleanBuild == true }
      }
      steps{
        echo("Refresh")
        script{
          currentBuild.result ="UNSTABLE"
        }
      }
    }
    stage('error') {
      when {
        expression { params.Refresh == false }
      }
      steps {
        lock('CCDAccess') {
          echo 'Do something here that requires unique access to the resource'
          // any other build will wait until the one locking the resource leaves this block
          sleep 30
        }
        echo "0 : ${JOB_NAME}"
        echo "A : ${JOB_BASE_NAME}"
        echo "B : ${params.entryName}"
        echo "B : ${params.CleanBuild}"
        echo "I : ${params.RequestUser}"
        echo "C : ${currentBuild.projectName}"
        echo "C : ${currentBuild.fullProjectName}"
        echo "D : ${currentBuild.displayName}"
        echo "F : ${currentBuild.fullDisplayName}"
        echo "H : ${MAIN_PROJECT_NAME}"
        echo "I : ${PLATFORM}"
        echo "I : ${GIT_COMMIT}"
      }
    }
    stage ('if'){
      when {
        expression { params.Refresh == false }
      }
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
  post {
      always {
          echo 'End Build'
      }
      success {
          echo "success"
      }
      unstable {
          echo 'I am unstable :/'
      }
  }
}