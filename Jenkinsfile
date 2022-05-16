pipeline {
  agent any
  stages {
    stage('error') {
      steps {
        echo "A : ${JOB_BASE_NAME}"
        echo "B : ${BRANCH_NAME}"
        echo "C : ${currentBuild.projectName}"
        echo "D : ${currentBuild.displayName}"
        echo "E : ${project.parent.displayName}"
        echo "F : ${currentBuild.rawBuild.project}"
      }
    }

  }
}