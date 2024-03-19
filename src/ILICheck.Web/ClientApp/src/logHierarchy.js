import groupBy from "object.groupby";

const constraintPatterns = [
  ["Mandatory Constraint", /^validate mandatory constraint (\S+)\.\.\.$/, /^Mandatory Constraint (\S+) is not true\.$/],
  [
    "Plausibility Constraint",
    /^validate plausibility constraint (\S+)\.\.\.$/,
    /^Plausibility Constraint (\S+) is not true\.$/,
  ],
  ["Existence Constraint", /^validate existence constraint (\S+)\.\.\.$/, /^Existence constraint (\S+) is violated/],
  ["Uniqueness Constraint", /^validate unique constraint (\S+)\.\.\.$/, /^Unique constraint (\S+) is violated/],
  ["Set Constraint", /^validate set constraint (\S+)\.\.\.$/, /^Set Constraint (\S+) is not true\.$/],
];

const customConstraintMessagePattern = /^(.*) (\w+\.\w+\.\w+\.\w+) \(.*\)$/;
const constraintNamePattern = /\s(\w+\.\w+\.\w+\.\w+)\s/;

export function createLogHierarchy(logData) {
  const constraintEntries = collectLogEntries(logData);
  return buildHierarchicalLogEntries(constraintEntries.values());
}

function collectLogEntries(entries) {
  const constraintEntries = new Map();

  for (const entry of entries) {
    let constraintFound = false;
    for (const [name, infoPattern, errorPattern] of constraintPatterns) {
      let match;
      if (entry.type === "Info") {
        match = infoPattern.exec(entry.message);
      } else if (entry.type === "Error") {
        match = errorPattern.exec(entry.message);
      } else {
        continue;
      }

      if (match) {
        const constraintName = match[1];
        addOrReplaceLogEntry(constraintEntries, name, constraintName, entry);
        constraintFound = true;
        break;
      }
    }

    if (!constraintFound && entry.type !== "Info") {
      const match = customConstraintMessagePattern.exec(entry.message);
      if (match) {
        entry.message = match[1];
        const constraintName = match[2];
        addOrReplaceLogEntry(constraintEntries, "", constraintName, entry);
      } else {
        const nameMatch = constraintNamePattern.exec(entry.message);
        if (nameMatch) {
          const constraintName = nameMatch[1];
          addOrReplaceLogEntry(constraintEntries, "", constraintName, entry);
        }
      }
    }
  }

  return constraintEntries;
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

function buildHierarchicalLogEntries(constraintEntries) {
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
