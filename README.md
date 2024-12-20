# Coding Interview

## Notes

### DB

I've created my own DB tables to progress with development, I've called these

1. `markShipment`
1. `markShipment_Lines`

Without access to the 'real' database tables / scripts I've had to make a few assumptions about how the DB should be structured.

1. I've used the shipmentID as the primary key for the markShipment table.  This allows for testing of SQL addition errors by attempting to insert an entry already in the table.

Also I've assumed that sanitation of input is required, as although the API endpoint is protected by an API key, it is for external use.  

1. shipmentID contains only alphanumerics in the sample JSON, but to make the sanitation a bit more interesting, I have assumed hyphens are also allowable.
1. sku is sanitized as alphanumerics only.
1. I have assumed that a attempting to write a sanitized string is preferable to failing validation for invalid characters.

### Retry Logic

The retry logic waits in function, as suggested by the infrastructure diagram, as opposed to overcomplicating the functionapp by rescheduling a message back into the queue.
- I have implemented my own recursive retry method
- I could have used Polly for out of the box retry (and in fact did in a past version)

