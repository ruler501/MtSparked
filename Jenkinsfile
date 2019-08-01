PROJECTS = ["MtSparked/MtSparked.Interop", "MtSparked/MtSparked.Core", MtSparked/MtSparked.UI", \
            "MtSparked/Services/MtSparked.Services.AspNetCore", "MtSparked/Services/MtSparked.Services.CouchBaseLite", \
			"MtSparked/Tools/MtSparked.Tools.UpdateDatabase"]
TEST_PROJECTS = ["MtSparked/Tests/MtSparked.Core.Tests", "MtSparked/Tests/MtSparked.Interop.Tests", \
				 "MtSparked/Tests/MtSparked.UI.Tests", "MtSparked/Tests/Services/MtSparked.Services.CouchBaseLite.Tests"]

def command_in_each(list, cmd) {
    for (int i = 0; i < list.size(); i++) {
        dir(${list[i]}) {
			sh ${cmd}
		}
    }
}

pipeline {

	agent any

	options {
		timestamps()
	}
	
	stages {
		stage('Build') {
			steps {
				command_in_each(PROJECTS, "dotnet build")
			}
		}
		
		stage('Test') {
			steps {
				command_in_each(TEST_PROJECTS, "dotnet test")
			}
		}
	}
	
	triggers {
        githubBranches events: [commit([])], spec: '', triggerMode: 'HEAVY_HOOKS'
        githubPullRequests events: [Open(), commitChanged()], spec: '', triggerMode: 'HEAVY_HOOKS'
    }
}