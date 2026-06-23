SLN = FlowTrack.slnx

.PHONY: test test-unit test-integration test-e2e

test:
	dotnet test $(SLN)

test-unit:
	dotnet test $(SLN) --filter "FullyQualifiedName!~IT&FullyQualifiedName!~E2E"

test-integration:
	dotnet test $(SLN) --filter "FullyQualifiedName~IT."

test-e2e:
	dotnet test $(SLN) --filter "FullyQualifiedName~E2E."
