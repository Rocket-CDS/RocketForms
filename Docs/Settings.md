# Settings

## From Email
This is the email address that the form uses to send the email.  Usually the SMTP email, but if the DNS zone has been setup with a SPF record if can be different.

## Manager Email
The is the email address that the form email will be sent to.

## ReplyTo Email 

The replyto email will be set to the email of the manager unless a "replytoemail" field exists int he settings.  
```
@TextBox(info, "genxml/textbox/replytoemail", "type='email' required")
```

## Copy to the sender
Copying the email to the sender has been disabled, this is to prevent spam issues with the SMTP server used to send the form.  

## Email Language
The language that the email will be sent.  Default is en-US.