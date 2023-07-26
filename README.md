# RocketForms

RocketForms sends emails with data that has been filled in on a website. Like a contact form or administration form.  



## Settings

### From Email
This is the email address that the form uses to send the email.  Usually the SMTP email, but if the DNS zone has been setup with a SPF record if can be different.

### Manager Email
The is the email address that the form email will be sent to.

### ReplyTo Email 

The replyto email will be sent linked to the email sent to the manager, so they can reply easily to the email.  There should be a field in the form which defines what the replyto email is.  
If this input field on the form does not exists then the manager email is used, to prevent spam issues with the SMTP server.

```
@TextBox(info, "genxml/textbox/email", "type='email' required")
```
or
```
@TextBox(info, "genxml/textbox/replytoemail", "type='email' required")
```

## Copy to the sender
Copying the email to the sender has been disabled, this is to prevent spam issues with the SMTP server used to send the form.  
