import groupBy from "object.groupby";

const constraintPatterns = [
  ["Mandatory Constraint", /^validate mandatory constraint (\S+)\.\.\.$/],
  ["Plausibility Constraint", /^validate plausibility constraint (\S+)\.\.\.$/],
  ["Existence Constraint", /^validate existence constraint (\S+)\.\.\.$/],
  ["Uniqueness Constraint", /^validate unique constraint (\S+)\.\.\.$/],
  ["Set Constraint", /^validate set constraint (\S+)\.\.\.$/],
];

// e.g. "Custom message ModelA.TopicA.ClassA.ConstraintName (ILI syntax)" will result in "Custom message"
const customConstraintMessagePattern = /^(.*) (\w+\.\w+\.\w+\.\w+) \(.*\)$/;
// e.g. " ModelA.TopicA.ClassA.ConstraintName (ILI syntax) " (ILI syntax is optional and will be removed from the message)
const constraintNamePattern = /\s(\w+\.\w+\.\w+\.\w+)(\s\(.+\))?\s/;

export function createLogHierarchy(logData) {
  const [constraintEntries, otherEntries] = collectLogEntries(logData);
  return buildHierarchicalLogEntries(constraintEntries.values(), otherEntries);
}

function collectLogEntries(entries) {
  const constraintEntries = new Map();
  const otherWarnings = new Set();
  const otherErrors = new Set();

  for (const entry of entries) {
    if (entry.type === "Info") {
      for (const [name, infoPattern] of constraintPatterns) {
        const match = infoPattern.exec(entry.message);
        if (match) {
          const constraintName = match[1];
          addOrReplaceLogEntry(constraintEntries, name, constraintName, entry);
          break;
        }
      }
    } else {
      const customMessageMatch = customConstraintMessagePattern.exec(entry.message);
      if (customMessageMatch) {
        entry.message = customMessageMatch[1];
        const constraintName = customMessageMatch[2];
        addOrReplaceLogEntry(constraintEntries, "", constraintName, entry);
      } else {
        const nameMatch = constraintNamePattern.exec(entry.message);
        if (nameMatch) {
          const constraintName = nameMatch[1];
          const iliSyntax = nameMatch[2];
          if (iliSyntax) {
            entry.message = entry.message.replace(iliSyntax, "");
          }
          addOrReplaceLogEntry(constraintEntries, "", constraintName, entry);
        } else if (entry.type === "Error") {
          otherErrors.add(entry.message);
        } else {
          otherWarnings.add(entry.message);
        }
      }
    }
  }

  const errors = Array.from(otherErrors).map((message) => ({ message, type: "Error" }));
  const warnings = Array.from(otherWarnings).map((message) => ({ message, type: "Warning" }));
  return [constraintEntries, errors.concat(warnings)];
}

function addOrReplaceLogEntry(entries, contraintType, constraintName, entry) {
  const existingEntry = entries.get(constraintName);
  if (existingEntry) {
    existingEntry.type = entry.type;
    existingEntry.message = entry.message;
  } else {
    entries.set(constraintName, {
      name: constraintName,
      type: entry.type,
      contraintType,
      message: contraintType + " " + constraintName,
    });
  }
}

function buildHierarchicalLogEntries(constraintEntries, otherEntries) {
  const hierarchicalEntries = [];
  const modelGroups = groupBy(constraintEntries, (e) => getFirstPart(e.name));
  for (const modelName of Object.keys(modelGroups)) {
    const modelGroup = modelGroups[modelName];
    const modelEntry = {
      message: modelName,
      type: "Info",
      values: [],
    };
    hierarchicalEntries.push(modelEntry);

    const classGroups = groupBy(modelGroup, (e) => getAllExceptLastPart(e.name));
    for (const fullClassName of Object.keys(classGroups)) {
      const classGroup = classGroups[fullClassName];
      const className = fullClassName.substring(modelName.length + 1);
      const classEntry = {
        message: className,
        type: "Info",
        values: [],
      };
      modelEntry.values.push(classEntry);

      for (const item of classGroup) {
        const message = item.message;
        const type = item.type;

        // Propagate the highest log type (ok, warning or error)
        if (type === "Error") {
          classEntry.type = type;
          modelEntry.type = type;
        } else if (type === "Warning" && classEntry.type !== "Error") {
          classEntry.type = type;
        } else if (type === "Warning" && modelEntry.type !== "Error") {
          modelEntry.type = type;
        }

        classEntry.values.push({
          message: message.replaceAll(fullClassName + ".", ""),
          type: type,
        });
      }
    }
  }

  if (otherEntries.length > 0) {
    hierarchicalEntries.push({
      message: "Weitere Meldungen",
      type: otherEntries.find((e) => e.type === "Error") ? "Error" : "Warning",
      values: otherEntries,
    });
  }

  return hierarchicalEntries;
}

function getFirstPart(name) {
  const index = name.indexOf(".");
  return index === -1 ? name : name.substring(0, index);
}

function getAllExceptLastPart(name) {
  const index = name.lastIndexOf(".");
  return index === -1 ? name : name.substring(0, index);
}
