module default {
  type Person {
    property name -> str;
    property email -> str {
      constraint exclusive;
    }
    multi link hobbies -> Hobby;
  }
  type Hobby {
    property name -> str;
  }
}