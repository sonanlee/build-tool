pipeline {
  agent any
  environment {
      MY_PROJECT_NAME = "${JOB_NAME}"
  }
  stages {
    stage('error') {
      steps {
        script {
          def allJob = env.JOB_NAME.tokenize('/') as String[];
          MY_PROJECT_NAME = allJob[0];
        }

        echo "0 : ${JOB_NAME}"
        echo "A : ${JOB_BASE_NAME}"
        echo "B : ${BRANCH_NAME}"
        echo "C : ${currentBuild.projectName}"
        echo "C : ${currentBuild.fullProjectName}"
        echo "D : ${currentBuild.displayName}"
        echo "F : ${currentBuild.fullProjectName}"
        echo "G : ${MY_PROJECT_NAME}"
      }
    }

  }
}