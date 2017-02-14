# aztekweb.workflows
Helpful workflows for Umbraco Forms


- SendEncodedEmail
	-  Use this in place of the standard SendEmail workflow to HTML encode the user's input.  This will will prevent markup and script from being rendered in the email client.  The original user input is saved as-is (unless modified by another workflow).

**General Links**

- [Umbraco Forms](https://our.umbraco.org/documentation/Add-ons/UmbracoForms/)
- [Adding a workflow](https://our.umbraco.org/documentation/Add-ons/UmbracoForms/Developer/Extending/Adding-a-Workflowtype)