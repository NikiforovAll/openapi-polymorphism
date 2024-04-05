#!/bin/bash

kiota show \
    --openapi http://localhost:5077/swagger/v1/swagger.json

kiota generate -l CSharp \
    --log-level trace \
    --output ./Example.Sdk \
    --namespace-name Example.Sdk \
    --class-name CompositeClient \
    --exclude-backward-compatible \
    --openapi http://localhost:5077/swagger/v1/swagger.json