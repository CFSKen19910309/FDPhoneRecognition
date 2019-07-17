tcp server return:
-1, unknown error
0, success
1, current task is still running, cannot accept another command
2, received "Abort", but no current task to be aborted.
3, received "Abort", abort the current task 
4, received "MMI", return the  "ACK MMI {station name}"
5, received "QueryPMP", but not receive command "MMI"