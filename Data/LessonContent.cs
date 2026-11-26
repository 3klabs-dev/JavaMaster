using JavaMaster.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace JavaMaster.Api.Data;

/// <summary>
/// The lesson content for the "Learn" path. This code is the single source of truth:
/// the questions, the answers and the feedback all live here and are written into the
/// database on start-up, so the browser can never see an answer before it has earned it.
///
/// Every Explanation is written as a nudge, not as the answer. It points at the rule the
/// learner should apply, so a wrong answer still makes them think instead of handing
/// over the correct option.
/// </summary>
public static class LessonContent
{
    public sealed record Q(
        string Code,
        QuestionType Type,
        string Topic,
        string Prompt,
        object Payload,
        object AnswerKey,
        string Explanation);

    public sealed record Lesson(int NodeOrder, string NodeTitle, string IntroText, string OutroText, Q[] Questions);

    public static readonly Lesson[] Lessons =
    {
        // ---------------------------------------------------------------- Hello World
        new(1, "Hello World",
            "Welcome to your first Java lesson! 🚀\n\nA program can talk to the player by printing text on the screen. In this lesson you will print your very first line, one small step at a time. No prior experience needed.",
            "You can now make Java print text on the screen. Next, you'll learn how a program remembers values by giving them names.",
            new[]
        {
            new Q("hello-world.q1", QuestionType.MultipleChoice, "Hello World",
                "Programs talk to us by printing text. In Java, the line that prints looks like this:\n\n<b>System.out.println(...);</b>\n\nRead it in two parts: <b>System.out</b> points at the screen, and <b>println</b> means \"print a line\".\n\nWhich of these two lines actually shows text on the screen?",
                new { options = new[] {
                    new { id = "a", text = "System.out.println(\"Hello\");" },
                    new { id = "b", text = "String message = \"Hello\";" } } },
                new { correctOptionId = "a", correctMessage = "Exactly! println shows the text, then starts a new line." },
                "Close! One line stores the text in a variable, but storing is not showing. Find the line that prints."),

            new Q("hello-world.q2", QuestionType.FillBlank, "Hello World",
                "Now let's print your own words.\n\nThe line below already has the Java parts. Only the text inside the quotation marks is missing.\n\nType exactly: <b>Hello World!</b>\n\nThe quotation marks tell Java that this is text, so type only what goes between them.",
                new { template = new[] { "System", ".", "out", ".", "println", "(", "\"{{blank}}\"", ")", ";" } },
                new { accepted = new[] { "Hello World!" }, caseSensitive = true,
                      output = "Hello World!", correctMessage = "That's it! Java prints exactly the letters inside the quotes." },
                "Almost! Match the capital letters, spaces and the ! exactly. Java prints precisely what is inside the quotes."),

            new Q("hello-world.q3", QuestionType.ArrangeTokens, "Hello World",
                "Your turn to build the whole line from scratch.\n\nPut the pieces in order to print <b>Hello World!</b>.\n\nStart with the part that names the screen (<b>System.out</b>), then choose the action (<b>println</b>), put the text inside the parentheses, and finish with a semicolon (;).",
                new { tokens = new[] { "System", "out", "println", "(", ")", "\"Hello World!\"", ";" },
                      lines = new[] { new[] { "{{1}}", ".", "{{2}}", ".", "{{3}}", "{{4}}", "{{5}}", "{{6}}", "{{7}}" } } },
                new { order = new[] { "System", "out", "println", "(", "\"Hello World!\"", ")", ";" },
                      output = "Hello World!", correctMessage = "Perfect - that is the line in the order Java reads it." },
                "Not quite. Read it like a sentence: System, then out, then println, then the text in ( ), then ;."),

            new Q("hello-world.q4", QuestionType.MultipleChoice, "Hello World",
                "One tiny difference changes the ending.\n\n<b>println</b> prints the text and then presses Enter, so the next output starts on a new line. <b>print</b> stays exactly where it is.\n\nWhich line leaves the next output on the same line?",
                new { options = new[] {
                    new { id = "a", text = "System.out.println(\"Welcome\");" },
                    new { id = "b", text = "System.out.print(\"Welcome\");" } } },
                new { correctOptionId = "b", correctMessage = "Yes! print leaves the next output on the same line." },
                "Almost! println moves to a new line at the end. Choose the one that stays on the same line."),

            new Q("hello-world.q5", QuestionType.FillSlots, "Hello World",
                "Last step - two lines working together.\n\nWe want the screen to show:\n\n<b>The result is: 42</b>\n\nBecause 42 must stay on the same line as the words, the first line must not press Enter. The second line finishes the sentence.\n\nFill in the two printing words.",
                new { lines = new[] {
                        new[] { "System", ".", "out", ".", "{{1}}", "(", "\"The result is: \"", ")", ";" },
                        new[] { "System", ".", "out", ".", "{{2}}", "(", "42", ")", ";" } },
                      bank = new[] { "println", "print" } },
                new { slots = new Dictionary<string, string> { ["1"] = "print", ["2"] = "println" },
                      output = "The result is: 42", correctMessage = "Excellent! The first line stays put; the second finishes it." },
                "Close! The first line must stay on the same line; the second one finishes it.")
        }),

        // ------------------------------------------------------------------- Variables
        new(2, "Variables",
            "Welcome to lesson 2! 🚀\n\nA program needs to remember things while it runs: a score, a name, whether a level is finished. In Java, each value lives in a named box called a <b>variable</b>. In this lesson you will create your first boxes and put values inside them.",
            "You can now store values and give them names. Next, you'll choose the right kind of box for each value so Java knows what it is holding.",
            new[]
        {
            new Q("variables.q1", QuestionType.MultipleChoice, "Variables",
                "A variable is a named box that holds a value. To create one, Java wants three things in this order:\n\n<b>type</b>, then <b>name</b>, then <b>value</b>.\n\nFor example, <b>int score = 100;</b> means: make an <em>int</em> box called <em>score</em> and put <em>100</em> in it.\n\nWhich line creates a box called age that holds 20?",
                new { options = new[] {
                    new { id = "a", text = "int age = 20;" },
                    new { id = "b", text = "age = 20;" },
                    new { id = "c", text = "variable int age = 20;" } } },
                new { correctOptionId = "a", correctMessage = "Exactly! Type, then name, then value." },
                "Close! Compare each option with the example. Which one has all three parts in the right place?"),

            new Q("variables.q2", QuestionType.FillBlank, "Variables",
                "Now pick the right kind of box.\n\nWhole numbers, like 20, use <b>int</b>. Numbers with a decimal part, like 9.99, use <b>double</b>.\n\nFill in the type for 9.99.",
                new { template = new[] { "{{blank}}", "price", "=", "9.99", ";" } },
                new { accepted = new[] { "double" }, caseSensitive = true,
                      correctMessage = "Yes! double is the usual type for decimals." },
                "Almost! The paragraph names the type for decimal numbers. Copy it exactly, including the capital letters."),

            new Q("variables.q3", QuestionType.FillSlots, "Variables",
                "Some values can only be one of two things: yes or no, on or off, finished or not. Java stores that in a <b>boolean</b>, which can only be <b>true</b> or <b>false</b>.\n\nLet's create a boolean called isReady and set it to true.\n\nBuild the line.",
                new { lines = new[] { new[] { "{{1}}", "{{2}}", "=", "{{3}}", ";" } },
                  bank = new[] { "boolean", "isReady", "true", "false", "int", "String", "null" } },
                new { slots = new Dictionary<string, string> { ["1"] = "boolean", ["2"] = "isReady", ["3"] = "true" },
                      correctMessage = "Nice! A boolean is always true or false." },
                "Close! The question says this box can only hold one of two values. Which type is built for exactly that?"),

            new Q("variables.q4", QuestionType.MultipleChoice, "Variables",
                "Variable names are for humans, so Java has a few simple rules:\n\n• they can use letters, digits, _ and $\n• they cannot start with a digit\n• they cannot be a Java keyword like int or class\n\nWhich of these names breaks a rule?",
                new { options = new[] {
                    new { id = "a", text = "_total" },
                    new { id = "b", text = "$rate" },
                    new { id = "c", text = "2ndValue" },
                    new { id = "d", text = "myScore" } } },
                new { correctOptionId = "c", correctMessage = "Right! A name cannot start with a digit." },
                "So close! Test each name against the three rules, one at a time."),

            new Q("variables.q5", QuestionType.FillSlots, "Variables",
                "Some values should never change, like the highest possible score. Java marks those with <b>final</b>, and programmers write their names in CAPITAL LETTERS by convention.\n\nThe highest score is a whole number, and it is 100.\n\nBuild the constant declaration.",
                new { lines = new[] { new[] { "final", "{{1}}", "{{2}}", "=", "{{3}}", ";" } },
                  bank = new[] { "int", "boolean", "MAX_SCORE", "MIN_SCORE", "100", "50", "final", "static" } },
                new { slots = new Dictionary<string, string> { ["1"] = "int", ["2"] = "MAX_SCORE", ["3"] = "100" },
                      correctMessage = "Perfect! final int MAX_SCORE = 100; cannot be changed later." },
                "Nice try! The paragraph explains what final does and how a constant's name should look. Check each blank against it.")
        }),

        // ----------------------------------------------------------------- Data Types
        new(3, "Data Types",
            "Welcome to lesson 3! 🚀\n\nLesson 2 gave you boxes. Now you'll choose the right kind of box for each value. Java has a small set of basic types, and most of the time you only need four: <b>int</b> for whole numbers, <b>double</b> for decimals, <b>char</b> for one character, and <b>boolean</b> for true or false. Let's meet them one at a time.",
            "You now know the basic types and when to use them. Next, you'll use operators to calculate and compare those values.",
            new[]
        {
            new Q("data-types.q1", QuestionType.MultipleChoice, "Data Types",
                "Start with a single character: one letter, one digit, or one symbol. Java stores it in a <b>char</b>.\n\nFor example, <b>char grade = 'A';</b> holds just the letter A.\n\nWhich type holds exactly one character?",
                new { options = new[] {
                    new { id = "a", text = "String" },
                    new { id = "b", text = "char" },
                    new { id = "c", text = "int" },
                    new { id = "d", text = "byte" } } },
                new { correctOptionId = "b", correctMessage = "Right! char holds exactly one character." },
                "Almost! Look at the example in the question. How many characters are inside the quotes?"),

            new Q("data-types.q2", QuestionType.FillBlank, "Data Types",
                "Whole numbers are usually stored in an <b>int</b>. But some numbers are too big for int. For those, Java has <b>long</b>.\n\nTo tell Java \"this number is a long\", add the letter <b>L</b> right after the digits. For example, <b>long total = 42L;</b>\n\nFill in the missing letter for this line.",
                new { template = new[] { "long", "total", "=", "42", "{{blank}}", ";" } },
                new { accepted = new[] { "L", "l" }, caseSensitive = false,
                      correctMessage = "Correct! 42L tells Java this is a long." },
                "Nearly there! Compare your line with the example. What did the example add after the digits?"),

            new Q("data-types.q3", QuestionType.FillSlots, "Data Types",
                "Decimals like 3.14 are stored in a <b>double</b>. That is Java's usual choice for decimal numbers.\n\nBuild the line that stores 3.14 in a variable called pi.",
                new { lines = new[] { new[] { "{{1}}", "{{2}}", "=", "3.14", ";" } },
                  bank = new[] { "double", "float", "pi", "int", "long", "3.14", "3.14f" } },
                new { slots = new Dictionary<string, string> { ["1"] = "double", ["2"] = "pi" },
                      correctMessage = "Nice! double pi = 3.14; stores the decimal." },
                "Close! The paragraph describes the kind of number 3.14 is. Choose the type for that kind, then the name."),

            new Q("data-types.q4", QuestionType.MultipleChoice, "Data Types",
                "Here's a surprise: when both numbers are whole, Java divides them as whole numbers and throws away the decimal part.\n\nSo 9 / 2 does not give 4.5. It gives the whole-number result, with the remainder dropped.\n\nWhat does <b>int x = 9; int y = 2; System.out.println(x / y);</b> print?",
                new { options = new[] {
                    new { id = "a", text = "4.5" },
                    new { id = "b", text = "4" },
                    new { id = "c", text = "5" },
                    new { id = "d", text = "Error" } } },
                new { correctOptionId = "b", correctMessage = "Right! 9 / 2 is 4; the .5 is dropped." },
                "Good try! Re-read the rule in the question, then do the division the way Java would, not the way a calculator would."),

            new Q("data-types.q5", QuestionType.FillSlots, "Data Types",
                "Two more types to finish.\n\nA <b>char</b> is one character and goes in <b>single quotes</b>: <b>'A'</b>. A <b>boolean</b> is either <b>true</b> or <b>false</b>.\n\nStore the letter A in <b>letter</b>, and set <b>finished</b> to false because the work is not done yet.\n\nFill in the two values.",
                new { lines = new[] {
                        new[] { "char", "letter", "=", "{{1}}", ";" },
                        new[] { "boolean", "finished", "=", "{{2}}", ";" } },
                  bank = new[] { "'A'", "A", "true", "false", "'B'", "'1'" } },
                new { slots = new Dictionary<string, string> { ["1"] = "'A'", ["2"] = "false" },
                      correctMessage = "Perfect! Single quotes for a char, false for unfinished work." },
                "Almost! Use the example for the character, and think about what \"not finished yet\" means for the second value.")
        }),

        // ----------------------------------------------------- Operators & Expressions
        new(5, "Operators & Expressions",
            "Welcome to lesson 4! 🚀\n\nYou can store values now. The next step is to do something with them: add them, compare them, or change them. The symbols that do this are called <b>operators</b>, and a calculation they form is an <b>expression</b>. We'll try the most useful ones one at a time.",
            "You can now calculate and compare values. Next, your programs will use these comparisons to make decisions.",
            new[]
        {
            new Q("operators-expressions.q1", QuestionType.MultipleChoice, "Operators & Expressions",
                "Most operators look like math: <b>+</b> adds, <b>-</b> subtracts, <b>*</b> multiplies, <b>/</b> divides.\n\nOne more gives you what is left over after a division. For example, 7 % 2 is 1, because 7 is 6 plus 1 left over.\n\nWhich operator gives the remainder?",
                new { options = new[] {
                    new { id = "a", text = "/" },
                    new { id = "b", text = "%" },
                    new { id = "c", text = "*" },
                    new { id = "d", text = "+" } } },
                new { correctOptionId = "b", correctMessage = "Right! % gives the remainder." },
                "One of these is the remainder operator. Re-read the example with 7 and 2, then compare it with the options."),

            new Q("operators-expressions.q2", QuestionType.FillBlank, "Operators & Expressions",
                "Dividing whole numbers has a catch. <b>5 / 2</b> gives <b>2</b>, not 2.5, because Java drops the decimal part.\n\nTo get 2.5, you can temporarily turn one number into a decimal. That is called a <b>cast</b>: put a type in parentheses before the value.\n\nJava's two decimal types are <b>float</b> and <b>double</b>. Fill in one of them.",
                new { template = new[] { "System", ".", "out", ".", "println", "(", "(", "{{blank}}", ")", "a", "/", "b", ")", ";" } },
                new { accepted = new[] { "double", "float" }, caseSensitive = true,
                      output = "2.5", correctMessage = "Correct! The cast forces decimal division." },
                "Either decimal type works here. Pick one from the paragraph and spell it exactly."),

            new Q("operators-expressions.q3", QuestionType.FillSlots, "Operators & Expressions",
                "Java has shortcuts for changing a value. <b>a += 3</b> means <b>a = a + 3</b>: add three, then keep the result.\n\nSuppose a is 5. Build the line that adds 3 to a and prints the new value.",
                new { lines = new[] { new[] { "System", ".", "out", ".", "println", "(", "a", "{{1}}", "3", ")", ";" } },
                  bank = new[] { "+=", "-=", "*=", "=", "++" } },
                new { slots = new Dictionary<string, string> { ["1"] = "+=" },
                      output = "8", correctMessage = "Nice! a += 3 adds 3 and prints 8." },
                "The paragraph describes a shortcut that adds and keeps the result. Match it to the options."),

            new Q("operators-expressions.q4", QuestionType.MultipleChoice, "Operators & Expressions",
                "Comparisons produce a boolean: <b>true</b> or <b>false</b>. For example, <b>5 > 2</b> is true.\n\nThe <b>&&</b> operator means <b>and</b>: the whole thing is true only if <em>both</em> sides are true.\n\nWhat does <b>int a = 5; int b = 2; System.out.println(a > b && b < a);</b> print?",
                new { options = new[] {
                    new { id = "a", text = "true" },
                    new { id = "b", text = "false" },
                    new { id = "c", text = "Error" },
                    new { id = "d", text = "1" } } },
                new { correctOptionId = "a", correctMessage = "Right! 5 > 2 and 2 < 5 are both true." },
                "Check each side of the && on its own first. What does each comparison give?"),

            new Q("operators-expressions.q5", QuestionType.FillSlots, "Operators & Expressions",
                "Two ways to add one:\n\n<b>x++</b> uses the old value first, then adds one.\n<b>++x</b> adds one first, then uses the new value.\n\nSuppose a is 5. Build the line that adds one <em>before</em> printing a.",
                new { lines = new[] { new[] { "System", ".", "out", ".", "println", "(", "{{1}}", "a", ")", ";" } },
                  bank = new[] { "++", "--", "+", "-" } },
                new { slots = new Dictionary<string, string> { ["1"] = "++" },
                      output = "6", correctMessage = "Exactly! ++a increases a before it is used, so it prints 6." },
                "Compare the two forms in the paragraph. Which one uses the new value when it prints?")
        })
    };

    /// <summary>
    /// Writes the lesson content into the database. Existing questions are updated in place
    /// so their ids (and the learner's attempt history) stay intact.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db)
    {
        var nodes = await db.LearnNodes
            .Include(n => n.NodeQuestions).ThenInclude(nq => nq.Question)
            .ToListAsync();

        foreach (var lesson in Lessons)
        {
            var node = nodes.FirstOrDefault(n => n.SortOrder == lesson.NodeOrder && n.NodeType == NodeType.Lesson);
            if (node == null) continue;

            // The text of a lesson belongs to the database once it is there, so a
            // professor can rewrite it in the app without the code overwriting it.
            if (string.IsNullOrEmpty(node.IntroText)) node.IntroText = lesson.IntroText;
            if (string.IsNullOrEmpty(node.OutroText)) node.OutroText = lesson.OutroText;

            var topic = await db.Topics.FirstOrDefaultAsync(t => t.Title == lesson.NodeTitle);
            if (topic == null)
            {
                topic = new Topic { Title = lesson.NodeTitle };
                db.Topics.Add(topic);
                await db.SaveChangesAsync();
            }

            var links = node.NodeQuestions.OrderBy(nq => nq.SortOrder).ToList();

            // Questions that were seeded before codes existed are adopted rather
            // than duplicated, so upgrading does not throw the content away.
            for (int i = 0; i < lesson.Questions.Length && i < links.Count; i++)
            {
                var existing = links[i].Question;
                if (existing != null && existing.Code == null) existing.Code = lesson.Questions[i].Code;
            }
            await db.SaveChangesAsync();

            for (int i = 0; i < lesson.Questions.Length; i++)
            {
                var def = lesson.Questions[i];

                // The code provides the starting content. Once a question is in the
                // database it belongs to the database, so a professor who edits it in
                // the app does not lose that edit on the next start.
                var question = await db.Questions.FirstOrDefaultAsync(q => q.Code == def.Code);

                if (question == null)
                {
                    question = new Question
                    {
                        Code = def.Code,
                        Topic = topic,
                        Purpose = QuestionPurpose.Lesson,
                        Type = def.Type,
                        Prompt = def.Prompt,
                        Payload = Json(def.Payload),
                        AnswerKey = Json(def.AnswerKey),
                        Explanation = def.Explanation,
                        Hint = null,               // Learn mode has no power-ups.
                        Difficulty = 1,
                        IsActive = true
                    };
                    db.Questions.Add(question);

                    var link = new NodeQuestion { LearnNodeId = node.Id, Question = question, SortOrder = i + 1 };
                    db.NodeQuestions.Add(link);
                    node.NodeQuestions.Add(link);
                }
                else if (!node.NodeQuestions.Any(nq => nq.QuestionId == question.Id))
                {
                    // The question exists but is not in this node yet.
                    var link = new NodeQuestion { LearnNodeId = node.Id, Question = question, SortOrder = i + 1 };
                    db.NodeQuestions.Add(link);
                    node.NodeQuestions.Add(link);
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static string Json(object o) => System.Text.Json.JsonSerializer.Serialize(o);
}
