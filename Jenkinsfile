pipeline {
  agent any
  stages {
    stage('error') {
      steps {
        echo 'Hi : ${env.JOB_BASE_NAME}'
        echo 'Bye : ${env.BRANCH_NAME}'
      }
    }

  }
}