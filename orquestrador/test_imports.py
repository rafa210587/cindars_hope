#!/usr/bin/env python3
"""
Test script to verify all orchestrator imports work correctly
"""

import sys
from pathlib import Path

# Add parent to path so imports work
sys.path.insert(0, str(Path(__file__).parent.parent))

def test_imports():
    """Test that all modules can be imported"""
    errors = []

    # Test core modules
    modules_to_test = [
        ("logger", "ItemLogger"),
        ("queue", "build_spec_queue"),
        ("validation", "run_all_validations"),
        ("agent", "run_with_fallback"),
        ("spec_operations", "git_status"),
    ]

    print("Testing imports...")
    for module_name, func_name in modules_to_test:
        try:
            mod = __import__(f"orquestrador.{module_name}", fromlist=[func_name])
            func = getattr(mod, func_name, None)
            if func:
                print(f"✅ {module_name}.{func_name}")
            else:
                print(f"⚠️  {module_name} exists but {func_name} not found")
                errors.append(f"{module_name}.{func_name} not found")
        except ImportError as e:
            print(f"❌ {module_name}: {str(e)}")
            errors.append(str(e))
        except Exception as e:
            print(f"❌ {module_name}: {str(e)}")
            errors.append(str(e))

    # Test config files exist
    print("\nTesting config files...")
    configs = [
        "orquestrador_config.json",
        "config.json",
    ]

    for config in configs:
        path = Path(__file__).parent / config
        if path.exists():
            print(f"✅ {config}")
        else:
            print(f"❌ {config} not found")
            errors.append(f"{config} not found")

    # Test directories exist
    print("\nTesting directories...")
    dirs = [
        "logs",
        "../docs/agent_prompts/a_executar",
        "../docs/agent_prompts/executados",
        "../docs/agent_prompts/bloqueados",
    ]

    for dir_path in dirs:
        full_path = Path(__file__).parent / dir_path
        if full_path.exists():
            print(f"✅ {dir_path}")
        else:
            print(f"❌ {dir_path} not found")
            errors.append(f"{dir_path} not found")

    # Summary
    print(f"\n{'='*50}")
    if errors:
        print(f"❌ {len(errors)} issue(s) found:")
        for error in errors:
            print(f"  - {error}")
        return False
    else:
        print("✅ All imports OK!")
        print("✅ All configs OK!")
        print("✅ All directories OK!")
        print("\nOrchestrator v2.0 ready to use!")
        return True


if __name__ == "__main__":
    success = test_imports()
    sys.exit(0 if success else 1)
