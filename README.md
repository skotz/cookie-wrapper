# Cookie Wrapper

Cookie management has burned me a few times recently, so I wanted to document how the 
request and response cookie collections work and why that impacts how you should use them.

## The Problem

Typically I see cookies being updated like this:

```
private void UpdateCookieTheWrongWay(string name, string value)
{
    // Warning: don't do this!
    var cookie = HttpContext.Current.Request.Cookies[name];
    if (cookie == null)
    {
        cookie = new HttpCookie(name);
    }
    cookie.Value = cookie.Value + value;
    HttpContext.Current.Response.SetCookie(cookie);
}
```

And this works fine, assuming you only do it once per page load. 
But what if you need to modify this same cookie multiple times in a single request?

```
UpdateCookieTheWrongWay("bad", "A");
UpdateCookieTheWrongWay("bad", "B");
UpdateCookieTheWrongWay("bad", "C");
```

Here's a look into the Request and Response cookie collections (which you can verify with the debugger) after every update.

| Request      | Response      |
|--------------|---------------|
| null         | "bad" = "A"   |
| "bad" = "A"  | "bad" = "AB"  |
| "bad" = "AB" | "bad" = "ABC" |

Upon this first page load everything looks as you'd expect. 
You received a new cookie with the value "ABC".

So when we reload the page it should send us an updated cookie with the value "ABCABC", right?

| Request                       | Response       |
|-------------------------------|----------------|
| "bad" = "ABC"                 | "bad" = "ABCA" |
| "bad" = "ABC"; "bad" = "ABCA" | "bad" = "ABCB" |
| "bad" = "ABC"; "bad" = "ABCB" | "bad" = "ABCC" |

Wrong! So what's going on here? What's with the rogue duplicate cookie in the request collection?

When you create a response cookie, that same cookie is automatically added to both the request and the response.
If a *response* cookie with that same name already exists, it's overwritten; however, if a *request* cookie with that same name already exists, 
it's left alone and a second cookie with the same name (but a different value) is added.

You might be tempted to think the solution is to just delete the request cookie after the first read, but that actually doesn't work either.
For the rest of the page load, if you grab this cookie out of the request collection you'll get the *original value*, not the updated one.

## The Solution

To make sure you're always reading the most up-to-date version of a given cookie during a page load, you need to always check the response 
collection before the request collection. If the response already has a value, use that instead of the request value.

```
private void UpdateCookie(string name, string value)
{
    HttpCookie cookie;
    if (System.Web.HttpContext.Current?.Response?.Cookies?.AllKeys?.Contains(name) ?? false)
    {
        cookie = System.Web.HttpContext.Current?.Response?.Cookies?[name];
    }
    else
    {
        cookie = System.Web.HttpContext.Current?.Request?.Cookies?[name];
    }

    if (cookie == null)
    {
        cookie = new HttpCookie(name);
    }

    cookie.Value = cookie.Value + value;

    System.Web.HttpContext.Current.Response.SetCookie(cookie);
}
```

Now lets run our same test.

```
UpdateCookie("good", "A");
UpdateCookie("good", "B");
UpdateCookie("good", "C");
```

And there we go! Since we're getting the value from the up-to-date response cookie (instead of the undeletable request cookie) we're able to make the changes correctly.

```
| Request                          | Response          |
|----------------------------------|-------------------|
| "good" = "ABC"                   | "good" = "ABCA"   |
| "good" = "ABC"; "good" = "ABCA"  | "good" = "ABCAB"  |
| "good" = "ABC"; "good" = "ABCAB" | "good" = "ABCABC" |
```

## Usage

Here's how to use the Cookie Wrapper.


```
// Create the cookie wrapper
var cookieWrapper = new CookieWrapper();

// Get the current cookie value
var cookie = cookieWrapper.GetCookie("cookieName");

// Create a new cookie if it doesn't exist
if (cookie == null)
{
    cookie = new HttpCookie("cookieName");
}

// Change the cookie value
cookie.Value = "New Value";

// Update the cookie
cookieWrapper.SetCookie(cookie);
```