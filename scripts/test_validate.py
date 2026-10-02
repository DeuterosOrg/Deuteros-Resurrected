"""Protect CI from Godot's log-only failures and false success markers."""
import unittest
from validate import check_output, EDITOR_TEARDOWN


class ValidationOutputTests(unittest.TestCase):
    def test_engine_error_fails_even_with_success_marker(self):
        with self.assertRaises(RuntimeError):
            check_output('SMOKE OK\n\x1b[1;31mERROR:\x1b[0m missing texture', 'SMOKE OK')

    def test_native_crash_fails_even_after_passing_assertions(self):
        for message in ('handle_crash: Program crashed with signal 11',
                        'FATAL: Condition "!rc_owner" is true.'):
            with self.subTest(message=message), self.assertRaises(RuntimeError):
                check_output('REGRESSION RESULT: 1 passed, 0 failed\n' + message,
                             'REGRESSION RESULT: 1 passed, 0 failed')

    def test_missing_runner_does_not_count_as_success(self):
        with self.assertRaises(RuntimeError):
            check_output('Godot Engine', 'REGRESSION RESULT:')

    def test_failed_assertion_fails_even_with_summary(self):
        with self.assertRaises(RuntimeError):
            check_output('FAIL: inventory\nREGRESSION RESULT: 0 passed, 1 failed', 'REGRESSION RESULT:')

    def test_editor_exception_is_narrow_and_only_for_import(self):
        check_output(EDITOR_TEARDOWN + '\nIMPORT OK', 'IMPORT OK', editor=True)
        with self.assertRaises(RuntimeError):
            check_output(EDITOR_TEARDOWN, editor=False)
        with self.assertRaises(RuntimeError):
            check_output('ERROR: another editor failure\nIMPORT OK', 'IMPORT OK', editor=True)

    def test_clean_completion_and_headless_warning_pass(self):
        check_output('WARNING: Mouse is not supported by this display server.\nSMOKE OK', 'SMOKE OK')


if __name__ == '__main__':
    unittest.main()
