pipeline {
  agent any
  stages {
    stage('error') {
      steps {
        script {
          def allJob = env.JOB_NAME.tokenize('/') as String[];
          def projectName = allJob[0];
        }

        echo "A : ${JOB_BASE_NAME}"
        echo "B : ${BRANCH_NAME}"
        echo "C : ${currentBuild.projectName}"
        echo "C : ${currentBuild.fullprojectName}"
        echo "D : ${currentBuild.displayName}"
        echo "F : ${currentBuild.fullProjectName}"
        echo "G : ${projectName}"
      }
    }

  }
}