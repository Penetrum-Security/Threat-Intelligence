rule NewPass_bin {
    meta:
        author = "Generated by Malcore on 09-17-2020 (contact@penetrum.com)"
        ref = "https://penetrum.com"copyright = "Penetrum, LLC"

    strings:
        $specific1 = ",,,,$$$$,,,,TTTTLLLLDDDDLLLLTTTT,,,,$$$$,,,,"
        $matchable1 = "</security>"$matchable10 = "  '&GEO"
        $matchable11 = "  'n"
        $matchable12 = "  /."
        $matchable13 = "  /.$,+*"
        $matchable14 = "  /.M(*"
        $matchable15 = "  /.mp)"
        $matchable16 = "  04+"
        $matchable17 = "  </trustInfo>"
        $matchable18 = "  = hr13<<=&r% "
        $matchable19 = "  ?>-<;:"
        $matchable2 = "<security>"
        $matchable20 = "  ?>:<;:"
        $matchable21 = "  s(!"$matchable22 = " ! j"
        $matchable23 = " ! o"
        $matchable24 = " !##"
        $matchable25 = " !&'"
        $matchable26 = " !&'$%"
        $matchable3 = "  #-%$'j"
        $matchable4 = "  #g"
        $matchable5 = "  '&"
        $matchable6 = "  '&!X+*)(/."
        $matchable7 = "  '&#$+*"
        $matchable8 = "  '&%$"
        $matchable9 = "  '&+$+*q(/.,,"
    condition:
        1 of ($specific*) and 13 of ($matchable*)
}


rule NewPass_backdoor_bin{
    meta:
        author = "Generated by Malcore on 09-17-2020 (contact@penetrum.com)"
        ref = "https://penetrum.com"copyright = "Penetrum, LLC"

    strings:
        $matchable1 = "H"$matchable10 = " A_A]A\\^["
        $matchable11 = " A_A^A\\"
        $matchable12 = " A_A^A\\_^"
        $matchable13 = " A_A^A]"
        $matchable14 = " A_A^A]A\\_"
        $matchable15 = " A_A^A]A\\_^]"
        $matchable16 = " A_A^^"
        $matchable17 = " A_A^_"
        $matchable18 = " Base Class Array'"
        $matchable19 = " Complete Object Locator'"
        $matchable2 = " !.350:"
        $matchable20 = " L9A"
        $matchable21 = " Type Descriptor'"
        $matchable22 = " delete"
        $matchable23 = " delete[]"$matchable24 = " new"
        $matchable25 = " new[]"
        $matchable26 = " r>p1v!t,z8x-~8|?b,`"
        $matchable3 = " #?%lu"
        $matchable4 = " &(6'"
        $matchable5 = " (e/=>cP"
        $matchable6 = " 2>59:"
        $matchable7 = " A^]["
        $matchable8 = " A^^]"
        $matchable9 = " A^_^"
    condition:
        13 of ($matchable*)
}

rule NewPass_launcher_bin{
    meta:
        author = "Generated by Malcore on 09-17-2020 (contact@penetrum.com)"
        ref = "https://penetrum.com"
        copyright = "Penetrum, LLC"

    strings:
        $matchable1 = "H"
        $matchable10 = " (l93;-"
        $matchable11 = " )55!+2"
        $matchable12 = " 3,f"
        $matchable13 = " 41%31"
        $matchable14 = " 4;92r"
        $matchable15 = " =5x~m|"
        $matchable16 = " A^]["
        $matchable17 = " A^^]"
        $matchable18 = " A^_^"
        $matchable19 = " A^_^]["
        $matchable2 = "</security>"
        $matchable20 = " A_A\\^]["
        $matchable21 = " A_A^A\\"
        $matchable22 = " A_A^A\\_^"
        $matchable23 = " A_A^A\\_^]["
        $matchable24 = " A_A^A]"
        $matchable25 = " A_A^A]A\\_"
        $matchable26 = " A_A^A]A\\_^]"
        $matchable3 = "<security>"
        $matchable4 = "  ** 2i,%&"
        $matchable5 = "  </trustInfo>"
        $matchable6 = " !HG"
        $matchable7 = " &!22+="
        $matchable8 = " &(8((&,-9"
        $matchable9 = " &0$.C%3< >M"
    condition:
        13 of ($matchable*)
}