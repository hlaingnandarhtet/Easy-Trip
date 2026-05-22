#!/usr/bin/env python3
"""Split Blazor .razor files into .razor + .razor.cs + .razor.css (code-behind pattern)."""
from __future__ import annotations

import re
from pathlib import Path

CLIENT = Path(__file__).resolve().parents[1] / "DotNet8.EasyTrip.App.Client"
SKIP = {"_Imports.razor", "Routes.razor"}

STANDARD_USINGS = """using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;
"""

def get_namespace_and_class(razor_path: Path) -> tuple[str, str]:
    rel = razor_path.relative_to(CLIENT)
    class_name = razor_path.stem
    parts = list(rel.parts[:-1])
    if parts == ["Pages"]:
        ns = "DotNet8.EasyTrip.App.Client.Pages"
    elif parts and parts[0] == "Pages":
        sub = ".".join(parts[1:])
        ns = f"DotNet8.EasyTrip.App.Client.Pages.{sub}" if sub else "DotNet8.EasyTrip.App.Client.Pages"
    elif parts and parts[0] == "Layout":
        ns = "DotNet8.EasyTrip.App.Client.Layout"
    elif parts and parts[0] == "Components":
        ns = "DotNet8.EasyTrip.App.Client.Components"
    else:
        ns = "DotNet8.EasyTrip.App.Client"
    return ns, class_name


def extract_style_blocks(content: str) -> tuple[str, str]:
    styles: list[str] = []
    pattern = re.compile(r"<style>\s*(.*?)\s*</style>", re.DOTALL | re.IGNORECASE)

    def repl(m: re.Match) -> str:
        styles.append(m.group(1).strip())
        return ""

    markup = pattern.sub(repl, content)
    css = "\n\n".join(styles)
    css = css.replace("@@", "@")
    return markup.strip(), css


def extract_code_block(content: str) -> tuple[str, str | None]:
    idx = content.find("@code")
    if idx < 0:
        return content, None
    start = content.find("{", idx)
    if start < 0:
        return content, None
    depth = 0
    end = start
    for i in range(start, len(content)):
        c = content[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                end = i
                break
    code_body = content[start + 1 : end].strip()
    markup = (content[:idx] + content[end + 1 :]).strip()
    return markup, code_body


def ensure_inherits(markup: str) -> str:
    if "@inherits ComponentBase" in markup:
        return markup
    lines = markup.splitlines()
    insert_at = 0
    for i, line in enumerate(lines):
        if line.strip().startswith("@") and not line.strip().startswith("@using"):
            insert_at = i + 1
        elif insert_at > 0 and not line.strip().startswith("@"):
            break
    if insert_at == 0:
        return "@inherits ComponentBase\n\n" + markup
    lines.insert(insert_at, "@inherits ComponentBase")
    return "\n".join(lines)


def is_placeholder_cs(cs_path: Path) -> bool:
    if not cs_path.exists():
        return True
    text = cs_path.read_text(encoding="utf-8").strip()
    if not text:
        return True
    if "all logic is contained within" in text.lower():
        return True
    # empty partial with only braces
    if text.count("{") <= 2 and "Inject" not in text and "private" not in text and "protected" not in text:
        body = re.sub(r"[\s\{\};]", "", text)
        if body.replace("using", "").replace("namespace", "").replace("public", "").replace("partial", "").replace("class", ""):
            if len(re.findall(r"\w+", body)) < 8:
                return True
    return False


def build_cs_file(ns: str, class_name: str, code_body: str) -> str:
    return f"""{STANDARD_USINGS}
namespace {ns}
{{
    public partial class {class_name}
    {{
{indent_code(code_body, 8)}
    }}
}}
"""


def indent_code(code: str, spaces: int) -> str:
    pad = " " * spaces
    lines = code.splitlines()
    return "\n".join(pad + line if line.strip() else line for line in lines)


def merge_into_existing_cs(cs_text: str, code_body: str) -> str:
    marker = "\n    }\n}\n"
    if marker in cs_text:
        insert_at = cs_text.rfind(marker)
        indented = indent_code(code_body, 8)
        return cs_text[:insert_at] + "\n" + indented + cs_text[insert_at:]
    return build_cs_file(
        re.search(r"namespace\s+([\w.]+)", cs_text).group(1),
        re.search(r"partial class\s+(\w+)", cs_text).group(1),
        code_body,
    )


def process_file(razor_path: Path) -> list[str]:
    actions: list[str] = []
    content = razor_path.read_text(encoding="utf-8")
    markup, css = extract_style_blocks(content)
    markup, code_body = extract_code_block(markup)

    ns, class_name = get_namespace_and_class(razor_path)
    cs_path = razor_path.with_suffix(".razor.cs")
    css_path = razor_path.with_suffix(".razor.css")

    if code_body:
        if cs_path.exists() and not is_placeholder_cs(cs_path):
            merged = merge_into_existing_cs(cs_path.read_text(encoding="utf-8"), code_body)
            cs_path.write_text(merged, encoding="utf-8")
            actions.append(f"  ~ {cs_path.name} (merged)")
        else:
            cs_path.write_text(build_cs_file(ns, class_name, code_body), encoding="utf-8")
            actions.append(f"  + {cs_path.name}")
    elif not cs_path.exists():
        cs_path.write_text(
            f"{STANDARD_USINGS}\nnamespace {ns}\n{{\n    public partial class {class_name}\n    {{\n    }}\n}}\n",
            encoding="utf-8",
        )
        actions.append(f"  + {cs_path.name} (empty)")

    if css:
        existing_css = css_path.read_text(encoding="utf-8") if css_path.exists() else ""
        if css.strip() != existing_css.strip():
            css_path.write_text(css + "\n", encoding="utf-8")
            actions.append(f"  + {css_path.name}")

    if code_body or css:
        if "@page" in markup or "@layout" in markup or razor_path.parent.name in ("Pages", "Layout", "Components") or "Pages" in str(razor_path):
            markup = ensure_inherits(markup)
        razor_path.write_text(markup + "\n", encoding="utf-8")
        actions.append(f"  ~ {razor_path.name}")

    return actions


def main() -> None:
    changed: list[str] = []
    for razor in sorted(CLIENT.rglob("*.razor")):
        if razor.name in SKIP:
            continue
        rel = razor.relative_to(CLIENT)
        if rel.parts and rel.parts[0] not in ("Pages", "Layout", "Components"):
            continue
        actions = process_file(razor)
        if actions:
            changed.append(f"{rel}:\n" + "\n".join(actions))

    print(f"Processed {len(changed)} file(s):\n")
    print("\n".join(changed) if changed else "No changes.")


if __name__ == "__main__":
    main()
