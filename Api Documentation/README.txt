
General flow.

A call to  /api/auth  will return an anonymous account.
Then call /api/list_chan to get the current list of channels.
Client can now be considered initialized.

The account and channels can be periodically refreshed, once during app startup is fine.

-----------

to start call  /api/gapless  without an event or a stale event to join the live stream.  Passing an event id will return the future events.
The playlist is only available around 30 events into the future.  So you'll need to account for that if people skip too far, by ignoring the skip call and continuing to play.

After a song starts playing a callback to  /api/update_history  is required.
And if audio is paused a callback to /api/pause  is required.

--------
additional song info can be retrieved with a song_id

-------
rating songs requires a level 2 account.
and interacting with song_comments requires a level 3 account.

We use an anonymous account until people attempt to rate a song or interact with a song comment.
At which point we prompt them to register/validate  as required.










