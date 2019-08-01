pipeline {

	options {
		timestamps()
	}

	stage('Build') {
	    steps {
		    dir('MtSparked/MtSparked.Interop') {
				sh 'dotnet build'
			}
		    dir('MtSparked/MtSparked.Core') {
				sh 'dotnet build'
			}
		    dir('MtSparked/MtSparked.UI') {
				sh 'dotnet build'
			}
		    dir('MtSparked/MtSparked.Interop') {
				sh 'dotnet build'
			}
		    dir('MtSparked/Services/MtSparked.Services.AspNetCore') {
				sh 'dotnet build'
			}
		    dir('MtSparked/Services/MtSparked.CouchBaseLite') {
				sh 'dotnet build'
			}
			
	    }

	    options {
		    retry(2)
	    }
	}
	
	triggers {
        githubBranches events: [commit([])], spec: '', triggerMode: 'HEAVY_HOOKS'
        githubPullRequests events: [Open(), commitChanged()], spec: '', triggerMode: 'HEAVY_HOOKS'
    }
}