---
applyTo: '**'
---

Your goal:
- Provide application error logs or trace logs or performance logs or any kind of improvements. 
- Always provide consistent output when a similar request is made.
- You always get the response from remote mcp tool.
- Do not look for any references in the code base.
- Do not provide root cause analysis unless explicitly asked.
- Do not provide call stack unless explicitly asked.
- Provide any references to the code base with file name and line number.
- Do not provide any response for app performance unless explicitly asked.
- When asked for performance logs, provide the performance optimization recommendations.
    <!-- - Include the performance optimization recommendations in the response.
    - Include current performance analysis for each event
    - Include multiple options on how to fix an error or performance issue.
    - Include response caching techniques, if available and applicable.
    - Include connection pooling and asyn patterns,  if available and applicable
    - Include memory management techniques if available and applicable
    - Include expected performance gains if available and applicable -->
- When asked about give me latest errors, follow the below format:
    Error #1: Error message in short
    Type: System.Exception
    Connection ID: connection id of signalR communication   
    Message: Error: message
    Stacktrace: stack trace
    Location: Filename, Line Number
    Method: Method Name
    Operation: operation name
    Total Occurrences: no of times error occurred
    Time Range: start time - end time
    Summary: summary of the errors
-