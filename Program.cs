
using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

const string DefaultUrl =
    "https://app.cloudqa.io/home/AutomationPracticeForm";

string firstName = "Test User";
string gender = "Male";
string country = "India";

string targetUrl =
    args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("TARGET_URL")
    ?? DefaultUrl;

// ==================================================
// RESILIENT ELEMENT IDENTIFICATION
// ==================================================

static IWebElement FindBySemanticLabel(
    IWebDriver driver,
    string labelText,
    string expectedTag,
    string expectedType = "",
    string desiredValue = "")
{
    string script = """
        const [labelText, expectedTag, expectedType, desiredValue] = arguments;

        const normalize = value =>
            (value || "").replace(/\s+/g, " ").trim().toLowerCase();

        const isVisible = el =>
            el.getClientRects().length > 0 &&
            getComputedStyle(el).visibility !== "hidden";

        const matches = el => {
            if (el.tagName.toLowerCase() !== expectedTag.toLowerCase())
                return false;

            if (expectedType &&
                normalize(el.type) !== normalize(expectedType))
                return false;

            if (desiredValue &&
                normalize(el.value) !== normalize(desiredValue))
                return false;

            return isVisible(el) && !el.disabled;
        };

        const labels = Array.from(
            document.querySelectorAll("label, legend, span")
        ).filter(el =>
            isVisible(el) &&
            normalize(el.innerText || el.textContent) === normalize(labelText)
        );

        for (const label of labels) {

            // Strategy 1: Native label association.
            const associated = label.control ||
                (label.getAttribute("for")
                    ? document.getElementById(label.getAttribute("for"))
                    : null);

            if (associated && matches(associated))
                return associated;

            // Strategy 2: Search nearby containers.
            let parent = label;

            for (let depth = 0; depth < 5 && parent; depth++) {

                const candidates = Array.from(
                    parent.querySelectorAll("input, select, textarea")
                ).filter(matches);

                if (candidates.length === 1)
                    return candidates[0];

                if (candidates.length > 1 && desiredValue) {
                    const exact = candidates.filter(
                        el => normalize(el.value) === normalize(desiredValue)
                    );

                    if (exact.length === 1)
                        return exact[0];
                }

                parent = parent.parentElement;
            }
        }

        throw new Error(
            "Could not uniquely identify " + labelText
        );
        """;

    object? result = ((IJavaScriptExecutor)driver).ExecuteScript(
        script,
        labelText,
        expectedTag,
        expectedType,
        desiredValue
    );

    return result as IWebElement
        ?? throw new NoSuchElementException(
            "Unable to identify field: " + labelText
        );
}

// ==================================================
// MAIN PROGRAM
// ==================================================

try
{
    if (!Uri.TryCreate(
        targetUrl,
        UriKind.Absolute,
        out Uri? uri) ||
        (uri.Scheme != Uri.UriSchemeHttp &&
         uri.Scheme != Uri.UriSchemeHttps))
    {
        throw new ArgumentException("Invalid target URL.");
    }

    ChromeOptions options = new ChromeOptions();
    options.AddArgument("--start-maximized");

    using IWebDriver driver = new ChromeDriver(options);

    driver.Navigate().GoToUrl(uri);

    Console.WriteLine("Page loaded: " + driver.Url);

    // ==================================================
    // HTML ATTRIBUTE AND STRUCTURE MUTATION TEST
    // ==================================================

    Console.WriteLine("\nApplying HTML mutation tests...");

    string mutationScript = """
        const normalize = value =>
            (value || "").replace(/\s+/g, " ").trim().toLowerCase();

        const labels = Array.from(
            document.querySelectorAll("label")
        );

        const firstLabel = labels.find(
            el => normalize(el.innerText) === "first name"
        );

        const stateLabel = labels.find(
            el => normalize(el.innerText) === "state"
        );

        const firstInput = firstLabel?.control;
        const stateSelect = stateLabel?.control;

        if (!firstInput || !stateSelect) {
            throw new Error("Required controls not found.");
        }

        // Change First Name attributes.
        firstInput.id = "user_first_982";
        firstInput.name = "customer_field_982";
        firstInput.className = "renamed-control";
        firstLabel.htmlFor = firstInput.id;

        // Change Gender radio attributes.
        const radios = Array.from(
            document.querySelectorAll('input[type="radio"]')
        );

        radios.forEach((radio, index) => {
            radio.id = "choice_" + index + "_982";
            radio.name = "profile_choice_982";
            radio.className = "renamed-radio";
        });

        // Change State attributes.
        stateSelect.id = "region_select_982";
        stateSelect.name = "region_field_982";
        stateSelect.className = "renamed-control";
        stateLabel.htmlFor = stateSelect.id;

        // Locate the original form groups.
        const firstGroup = firstInput.closest(".form-group");
        const stateGroup = stateSelect.closest(".form-group");

        if (!firstGroup || !stateGroup) {
            throw new Error("Form groups not found.");
        }

        // Verify that both controls belong to the same form.
        if (firstGroup.closest("form") !== stateGroup.closest("form")) {
            throw new Error("Controls belong to different forms.");
        }

        // Add a wrapper around First Name.
        const firstWrapper = document.createElement("div");
        firstWrapper.className = "new-first-wrapper";

        firstInput.parentElement.insertBefore(
            firstWrapper,
            firstInput
        );

        firstWrapper.appendChild(firstInput);

        // Add a wrapper around State.
        const stateWrapper = document.createElement("div");
        stateWrapper.className = "new-state-wrapper";

        stateSelect.parentElement.insertBefore(
            stateWrapper,
            stateSelect
        );

        stateWrapper.appendChild(stateSelect);

        // Create a new container for the two form groups.
        const newContainer = document.createElement("div");
        newContainer.className = "restructured-form-container";

        const originalParent = firstGroup.parentElement;

        if (!originalParent) {
            throw new Error("Original parent not found.");
        }

        originalParent.insertBefore(newContainer, firstGroup);

        // Move State first, then First Name.
        // This deliberately changes their original order.
        newContainer.appendChild(stateGroup);
        newContainer.appendChild(firstGroup);

        return "Attribute and structural mutations completed.";
        """;

    object? mutationResult =
        ((IJavaScriptExecutor)driver).ExecuteScript(mutationScript);

    Console.WriteLine(mutationResult);

    // ==================================================
    // AUTOMATED FORM TESTING
    // ==================================================

    WebDriverWait wait = new WebDriverWait(
        driver,
        TimeSpan.FromSeconds(15)
    );

    Console.WriteLine("\nTesting semantic identification...");

    // TEST 1: FIRST NAME

    IWebElement firstNameField = wait.Until(d =>
        FindBySemanticLabel(
            d,
            "First Name",
            "input",
            "text"
        )
    );

    firstNameField.Clear();
    firstNameField.SendKeys(firstName);

    if (firstNameField.GetAttribute("value") != firstName)
    {
        throw new Exception("First Name verification failed.");
    }

    Console.WriteLine("PASS: First Name entered and verified.");

    // TEST 2: GENDER

    IWebElement genderField = FindBySemanticLabel(
        driver,
        "Gender",
        "input",
        "radio",
        gender
    );

    genderField.Click();

    if (!genderField.Selected)
    {
        throw new Exception("Gender verification failed.");
    }

    Console.WriteLine("PASS: Gender selected and verified.");

    // TEST 3: STATE DROPDOWN

    IWebElement stateField = FindBySemanticLabel(
        driver,
        "State",
        "select"
    );

    SelectElement stateDropdown = new SelectElement(stateField);

    stateDropdown.SelectByText(country);

    string selectedCountry =
        stateDropdown.SelectedOption.Text.Trim();

    if (!selectedCountry.Equals(
        country,
        StringComparison.OrdinalIgnoreCase))
    {
        throw new Exception("Dropdown verification failed.");
    }

    Console.WriteLine("PASS: Dropdown selected and verified.");

    // ==================================================
    // FINAL REPORT
    // ==================================================

    Console.WriteLine("\n====================================");
    Console.WriteLine("       AUTOMATION TEST REPORT");
    Console.WriteLine("====================================");

    Console.WriteLine("First Name : " +
        firstNameField.GetAttribute("value"));

    Console.WriteLine("Gender     : " +
        genderField.GetAttribute("value"));

    Console.WriteLine("Dropdown   : " +
        selectedCountry);

    Console.WriteLine("------------------------------------");
    Console.WriteLine("RESULT: ALL THREE TESTS PASSED");
    Console.WriteLine("====================================");

    Console.WriteLine("\nPress ENTER to close Chrome.");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine("\nAUTOMATION TEST FAILED");
    Console.WriteLine(ex);

    Environment.ExitCode = 1;
}
