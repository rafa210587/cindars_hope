"""
Claude API Integration - Handles calls to Claude/Codex for spec execution
"""

import json
import os
from pathlib import Path
from typing import Optional
from dataclasses import dataclass

import anthropic
from dotenv import load_dotenv


@dataclass
class SpecExecutionPrompt:
    """Structured prompt for spec execution"""
    spec_id: str
    spec_name: str
    task_name: str
    spec_file_path: str
    task_description: str
    blockers_resolved: list[str]
    project_root: str
    validation_required: bool = True


class ClaudeSpecExecutor:
    """Handles spec execution via Claude API"""

    def __init__(self, config_path: str = "config.json"):
        load_dotenv()
        self.api_key = os.getenv("ANTHROPIC_API_KEY")
        self.client = anthropic.Anthropic(api_key=self.api_key)

        # Load config
        with open(config_path, 'r') as f:
            self.config = json.load(f)

        self.model = self.config.get("api_config", {}).get("model", "claude-opus-4-7")
        self.temperature = self.config.get("api_config", {}).get("temperature", 0.7)
        self.max_tokens = self.config.get("api_config", {}).get("max_tokens", 4096)

    def build_execution_prompt(self, prompt: SpecExecutionPrompt) -> str:
        """Build detailed prompt for Claude to execute a spec task"""
        blockers_text = "\n".join(f"- {blocker}" for blocker in prompt.blockers_resolved) if prompt.blockers_resolved else "None"

        prompt_text = f"""
# SPEC EXECUTION REQUEST - {prompt.spec_id}

## Overview
You are Claude, integrated into an orchestrator for the Cindar's Hope game project.
Execute the following spec task as part of a multi-session implementation plan.

## Spec Details
- **Spec ID**: {prompt.spec_id}
- **Spec Name**: {prompt.spec_name}
- **Task**: {prompt.task_name}
- **Description**: {prompt.task_description}

## Spec Document
Location: {prompt.spec_file_path}

## Context
- **Project Root**: {prompt.project_root}
- **Blockers Resolved**:
{blockers_text}

## Critical Instructions
1. **Scope**: Execute ONLY this specific task, not the entire spec
2. **Implementation**: Follow CLAUDE.md patterns:
   - Use GameEventBus for communication (no direct calls)
   - All ScriptableObjects follow _XSO naming convention
   - Never use GameObject.Find() or FindObjectOfType()
   - Commits must be in Portuguese
   - Persist IDs and primitives, never Unity references
3. **Validation**: {f'Run validation tools after implementation' if prompt.validation_required else 'Skip validation for this task'}
4. **Output**:
   - Implement the feature
   - Run any required validation
   - Commit changes with clear message
   - Report completion status

## Memory & Skills
If similar work exists in prior specs, reference:
- memory/MEMORY.md for consolidated patterns
- memory/feedback_working_method.md for proven methods
- memory/project_skills_available.md for reusable patterns

## Success Criteria
- Code compiles without errors
- No regressions in other systems
- Task-specific validation passes
- Changes committed with Portuguese message

Begin implementation now. Do not ask for clarification - proceed with the best interpretation of the requirements.
"""
        return prompt_text

    def execute_spec_task(self, prompt: SpecExecutionPrompt) -> tuple[bool, str]:
        """
        Execute a spec task via Claude API.
        Returns (success: bool, response_text: str)
        """
        try:
            execution_prompt = self.build_execution_prompt(prompt)

            response = self.client.messages.create(
                model=self.model,
                max_tokens=self.max_tokens,
                temperature=self.temperature,
                messages=[
                    {
                        "role": "user",
                        "content": execution_prompt
                    }
                ]
            )

            response_text = response.content[0].text if response.content else ""
            success = "completed" in response_text.lower() or "✅" in response_text or "succeeded" in response_text.lower()

            return success, response_text

        except anthropic.APIError as e:
            return False, f"API Error: {str(e)}"
        except Exception as e:
            return False, f"Execution Error: {str(e)}"

    def generate_spec_summary(self, spec_id: str, spec_name: str, task_results: list[str]) -> str:
        """Generate summary of spec task completions"""
        summary_prompt = f"""
Generate a concise summary for spec {spec_id} ({spec_name}) that completed the following tasks:

{chr(10).join(f"- {result}" for result in task_results)}

Include:
1. What was accomplished
2. Key files modified
3. Tests that were run
4. Any blockers encountered
5. Ready for next specs

Format as markdown.
"""

        try:
            response = self.client.messages.create(
                model=self.model,
                max_tokens=1000,
                messages=[
                    {
                        "role": "user",
                        "content": summary_prompt
                    }
                ]
            )

            return response.content[0].text if response.content else "Summary generation failed"
        except Exception as e:
            return f"Error generating summary: {str(e)}"
